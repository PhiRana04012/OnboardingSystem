using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnboardingSystem.Data;

namespace OnboardingSystem.Services;

public class AiMentorService : IAiMentorService
{
    private const string KnowledgeContextCacheKey = "AiMentor:KnowledgeContext";
    private static readonly SemaphoreSlim GroqRequestGate = new(1, 1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AiMentorService> _logger;

    public AiMentorService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        AppDbContext context,
        IWebHostEnvironment environment,
        IMemoryCache cache,
        ILogger<AiMentorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _context = context;
        _environment = environment;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> AskAsync(
        string message,
        bool includeKnowledgeContext = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Пустой вопрос недопустим.", nameof(message));
        }

        var model = _configuration["AiMentor:Model"] ?? "mixtral-8x7b-32768";
        var baseUrl = _configuration["AiMentor:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
        var apiKey = _configuration["AiMentor:ApiKey"] ?? "";
        var maxContextChars = _configuration.GetValue("AiMentor:MaxContextChars", 8000);

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("AI Mentor API key is not configured");
            throw new InvalidOperationException("API ключ для AI-сервиса не настроен");
        }

        var systemPrompt = includeKnowledgeContext
            ? BuildSystemPrompt(await GetOrBuildKnowledgeContextAsync(maxContextChars, cancellationToken))
            : "Ты AI-наставник системы онбординга. Отвечай кратко на русском языке.";

        const int maxRetries = 3;
        var retryCount = 0;

        await GroqRequestGate.WaitAsync(cancellationToken);
        try
        {
            while (true)
            {
                var payload = new
                {
                    model,
                    messages = new object[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = message.Trim() }
                    },
                    temperature = 0.2,
                    max_tokens = 512
                };

                var client = _httpClientFactory.CreateClient("AiMentor");
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    _logger.LogInformation(
                        "Sending AI Mentor request (knowledge={IncludeKnowledge}, attempt {Attempt})",
                        includeKnowledgeContext,
                        retryCount + 1);

                    using var response = await client.PostAsync("chat/completions", content, cancellationToken);
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                    if ((int)response.StatusCode == 429)
                    {
                        if (retryCount < maxRetries)
                        {
                            var delaySeconds = ParseRetryAfterSeconds(responseBody)
                                ?? Math.Min(5 * (retryCount + 1), 30);
                            _logger.LogWarning(
                                "Rate limit reached. Waiting {Seconds}s before retry...",
                                delaySeconds);
                            await Task.Delay(delaySeconds * 1000, cancellationToken);
                            retryCount++;
                            continue;
                        }

                        _logger.LogError(
                            "AiMentor API rate limited after {Retries} retries: {Body}",
                            maxRetries,
                            responseBody);
                        throw new InvalidOperationException("AI-сервис перегружен. Попробуйте позже.");
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("AiMentor API error {Status}: {Body}", response.StatusCode, responseBody);
                        throw new InvalidOperationException($"AI-сервис ошибка {response.StatusCode}: {responseBody}");
                    }

                    using var doc = JsonDocument.Parse(responseBody);
                    var aiReply = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    if (string.IsNullOrWhiteSpace(aiReply))
                    {
                        return "Не удалось сгенерировать ответ. Попробуйте переформулировать вопрос.";
                    }

                    return aiReply.Trim();
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogError(ex, "AI Mentor request was cancelled");
                    throw new InvalidOperationException("Запрос к AI-сервису был отменён");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI service at {BaseUrl}", baseUrl);
            throw new InvalidOperationException($"Не удалось подключиться к AI-сервису на {baseUrl}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI Mentor response");
            throw new InvalidOperationException("Не удалось обработать ответ AI-сервиса");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AI Mentor service");
            throw;
        }
        finally
        {
            GroqRequestGate.Release();
        }
    }

    private async Task<string> GetOrBuildKnowledgeContextAsync(int maxChars, CancellationToken cancellationToken)
    {
        var cacheMinutes = _configuration.GetValue("AiMentor:KnowledgeCacheMinutes", 30);
        return await _cache.GetOrCreateAsync(KnowledgeContextCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheMinutes);
            return await BuildKnowledgeContextAsync(maxChars, cancellationToken);
        }) ?? string.Empty;
    }

    private async Task<string> BuildKnowledgeContextAsync(int maxChars, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        var faqItems = await _context.FaqEntries
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Question)
            .ToListAsync(cancellationToken);

        if (faqItems.Count > 0)
        {
            sb.AppendLine("FAQ:");
            foreach (var item in faqItems)
            {
                sb.AppendLine($"Q: {item.Question}");
                sb.AppendLine($"A: {item.Answer}");
            }
            sb.AppendLine();
        }

        foreach (var resolvedPath in GetKnowledgeFilePaths())
        {
            try
            {
                var text = ReadKnowledgeFile(resolvedPath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogDebug("Knowledge file is empty after extraction: {Path}", resolvedPath);
                    continue;
                }

                var trimmed = text.Length > maxChars / 2 ? text[..(maxChars / 2)] : text;
                sb.AppendLine($"[FILE: {Path.GetFileName(resolvedPath)}]");
                sb.AppendLine(trimmed);
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read knowledge file: {Path}", resolvedPath);
            }
        }

        var fullContext = sb.ToString();
        return fullContext.Length > maxChars ? fullContext[..maxChars] : fullContext;
    }

    private IEnumerable<string> GetKnowledgeFilePaths()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var configured = _configuration.GetSection("AiMentor:KnowledgePaths").Get<string[]>() ?? [];

        foreach (var path in configured)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            var resolved = ResolveKnowledgePath(path);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                result.Add(resolved);
            }
        }

        foreach (var dir in GetKnowledgeSearchDirectories())
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(dir, "*.pdf"))
            {
                result.Add(file);
            }
        }

        return result;
    }

    private IEnumerable<string> GetKnowledgeSearchDirectories()
    {
        yield return Path.Combine(_environment.ContentRootPath, "PDF");
        yield return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "docs"));
        yield return Path.Combine(_environment.ContentRootPath, "docs");
    }

    private string BuildSystemPrompt(string knowledgeContext)
    {
        return
            "Ты AI-наставник системы онбординга. Отвечай кратко и по делу на русском языке. " +
            "Используй только проверенный контекст из FAQ и документов ниже. " +
            "Если в контексте нет точного ответа, честно скажи об этом и предложи обратиться к HR/наставнику.\n\n" +
            $"Контекст знаний:\n{knowledgeContext}";
    }

    private string? ResolveKnowledgePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        var candidates = new[]
        {
            Path.Combine(_environment.ContentRootPath, configuredPath),
            Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", configuredPath))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var fileName = Path.GetFileName(configuredPath);
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var normalizedTarget = NormalizeFileName(fileName);
        foreach (var dir in GetKnowledgeSearchDirectories())
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(dir))
            {
                if (NormalizeFileName(Path.GetFileName(file)) == normalizedTarget)
                {
                    return file;
                }
            }
        }

        return null;
    }

    private static string NormalizeFileName(string name) =>
        string.Concat(name.Where(c => !char.IsWhiteSpace(c) && c != '\u00A0')).ToLowerInvariant();

    private static string ReadKnowledgeFile(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".md" => File.ReadAllText(path),
            ".txt" => File.ReadAllText(path),
            ".docx" => ExtractTextFromDocx(path),
            ".pdf" => ExtractTextFromPdf(path),
            _ => string.Empty
        };
    }

    private static string ExtractTextFromDocx(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var docEntry = archive.GetEntry("word/document.xml");
        if (docEntry == null)
        {
            return string.Empty;
        }

        using var stream = docEntry.Open();
        using var reader = new StreamReader(stream);
        var xml = reader.ReadToEnd();
        var plainText = Regex.Replace(xml, "<.*?>", " ");
        plainText = System.Net.WebUtility.HtmlDecode(plainText);
        plainText = Regex.Replace(plainText, "\\s+", " ").Trim();
        return plainText;
    }

    private static string ExtractTextFromPdf(string path)
    {
        try
        {
            var sb = new StringBuilder();

            using (var reader = new PdfReader(path))
            {
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    try
                    {
                        var text = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, page);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            sb.AppendLine(text);
                        }
                    }
                    catch
                    {
                        // skip unreadable page
                    }
                }
            }

            return sb.ToString();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static int? ParseRetryAfterSeconds(string responseBody)
    {
        var match = Regex.Match(
            responseBody,
            @"try again in (\d+(?:\.\d+)?)\s*s",
            RegexOptions.IgnoreCase);
        if (match.Success &&
            double.TryParse(
                match.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var seconds))
        {
            return (int)Math.Ceiling(seconds) + 1;
        }

        return null;
    }
}
