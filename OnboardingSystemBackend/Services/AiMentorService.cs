using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using OnboardingSystem.Data;

namespace OnboardingSystem.Services;

public class AiMentorService : IAiMentorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AiMentorService> _logger;

    public AiMentorService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        AppDbContext context,
        IWebHostEnvironment environment,
        ILogger<AiMentorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> AskAsync(string message, bool includeKnowledgeContext = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Пустой вопрос недопустим.", nameof(message));
        }

        var model = _configuration["AiMentor:Model"] ?? "mixtral-8x7b-32768";
        var baseUrl = _configuration["AiMentor:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
        var apiKey = _configuration["AiMentor:ApiKey"] ?? "";
        var maxContextChars = _configuration.GetValue("AiMentor:MaxContextChars", 12000);

        _logger.LogInformation("AI Mentor config - BaseUrl: {BaseUrl}, Model: {Model}, HasApiKey: {HasKey}", 
            baseUrl, model, !string.IsNullOrEmpty(apiKey));

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("AI Mentor API key is not configured");
            throw new InvalidOperationException("API ключ для AI-сервиса не настроен");
        }

        var knowledgeContext = includeKnowledgeContext 
            ? await BuildKnowledgeContextAsync(maxContextChars, cancellationToken)
            : string.Empty;
        var systemPrompt = BuildSystemPrompt(knowledgeContext);

        // OpenAI-совместимый формат (для Groq)
        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = message.Trim() }
            },
            temperature = 0.2,
            max_tokens = 1024
        };

        var client = _httpClientFactory.CreateClient("AiMentor");
        client.BaseAddress = new Uri(baseUrl);
        
        // Добавляем API ключ для Groq
        if (!string.IsNullOrEmpty(apiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            _logger.LogInformation("Sending AI Mentor request to {BaseUrl} with model {Model}", baseUrl, model);
            
            using var response = await client.PostAsync("chat/completions", content, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogInformation("AI Mentor response status: {Status}", response.StatusCode);
            
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AI Mentor service");
            throw;
        }
    }

    private async Task<string> BuildKnowledgeContextAsync(int maxChars, CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Starting BuildKnowledgeContext ===");
        var sb = new StringBuilder();

        var faqItems = await _context.FaqEntries
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Question)
            .ToListAsync(cancellationToken);

        if (faqItems.Count > 0)
        {
            _logger.LogInformation("Found {FaqCount} FAQ items", faqItems.Count);
            sb.AppendLine("FAQ:");
            foreach (var item in faqItems)
            {
                sb.AppendLine($"Q: {item.Question}");
                sb.AppendLine($"A: {item.Answer}");
            }
            sb.AppendLine();
        }

        var knowledgePaths = _configuration
            .GetSection("AiMentor:KnowledgePaths")
            .Get<string[]>() ?? Array.Empty<string>();

        _logger.LogInformation("Configured knowledge paths count: {PathCount}", knowledgePaths.Length);
        foreach (var path in knowledgePaths)
        {
            _logger.LogInformation("Processing knowledge path: {Path}", path);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            try
            {
                var resolvedPath = ResolveKnowledgePath(path);
                if (string.IsNullOrWhiteSpace(resolvedPath))
                {
                    _logger.LogWarning("Could not resolve knowledge path: {KnowledgePath}", path);
                    continue;
                }
                
                if (!File.Exists(resolvedPath))
                {
                    _logger.LogWarning("Knowledge file does not exist: {ResolvedPath}", resolvedPath);
                    continue;
                }

                var text = ReadKnowledgeFile(resolvedPath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Knowledge file is empty after extraction: {ResolvedPath}", resolvedPath);
                    continue;
                }

                _logger.LogInformation("Successfully loaded knowledge file: {FileName} ({Length} chars)", 
                    Path.GetFileName(resolvedPath), text.Length);
                sb.AppendLine($"[FILE: {Path.GetFileName(resolvedPath)}]");
                sb.AppendLine(text);
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read knowledge file: {KnowledgePath}", path);
            }
        }

        var fullContext = sb.ToString();
        return fullContext.Length > maxChars ? fullContext[..maxChars] : fullContext;
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
        _logger.LogInformation("Attempting to resolve path: {ConfiguredPath}", configuredPath);
        
        if (Path.IsPathRooted(configuredPath))
        {
            _logger.LogInformation("Path is absolute, using as-is: {Path}", configuredPath);
            return configuredPath;
        }

        var localPath = Path.Combine(_environment.ContentRootPath, configuredPath);
        _logger.LogInformation("Checking local path: {LocalPath}", localPath);
        if (File.Exists(localPath))
        {
            _logger.LogInformation("Found file at local path: {LocalPath}", localPath);
            return localPath;
        }

        var projectRootCandidate = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, ".."));
        var rootPath = Path.Combine(projectRootCandidate, configuredPath);
        _logger.LogInformation("Checking root path: {RootPath}", rootPath);
        if (File.Exists(rootPath))
        {
            _logger.LogInformation("Found file at root path: {RootPath}", rootPath);
            return rootPath;
        }

        _logger.LogError("File not found at any location. Configured: {ConfiguredPath}, Local: {LocalPath}, Root: {RootPath}", 
            configuredPath, localPath, rootPath);
        return null;
    }

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
            System.Diagnostics.Debug.WriteLine($"Starting PDF extraction from: {path}");
            var sb = new StringBuilder();
            
            using (var reader = new PdfReader(path))
            {
                System.Diagnostics.Debug.WriteLine($"PDF opened successfully. Total pages: {reader.NumberOfPages}");
                int extractedPages = 0;
                
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    try
                    {
                        var text = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, page);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            sb.AppendLine($"--- Page {page} ---");
                            sb.AppendLine(text);
                            extractedPages++;
                        }
                    }
                    catch (Exception pageEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error extracting page {page}: {pageEx.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Successfully extracted {extractedPages} pages from {path}. Total text: {sb.Length} chars");
            }
            
            return sb.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PDF extraction error from {path}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}, Stack: {ex.StackTrace}");
            return string.Empty;
        }
    }
}
