using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    public async Task<string> AskAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Пустой вопрос недопустим.", nameof(message));
        }

        var model = _configuration["AiMentor:Model"] ?? "mixtral-8x7b-32768";
        var baseUrl = _configuration["AiMentor:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
        var apiKey = _configuration["AiMentor:ApiKey"] ?? "";
        var maxContextChars = _configuration.GetValue("AiMentor:MaxContextChars", 12000);

        var knowledgeContext = await BuildKnowledgeContextAsync(maxContextChars, cancellationToken);
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
            using var response = await client.PostAsync("chat/completions", content, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("AiMentor API error {Status}: {Body}", response.StatusCode, responseBody);
                throw new InvalidOperationException("AI-сервис временно недоступен.");
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
            throw new InvalidOperationException($"Не удалось подключиться к AI-сервису. Убедитесь что Ollama запущен на {baseUrl}");
        }
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

        var knowledgePaths = _configuration
            .GetSection("AiMentor:KnowledgePaths")
            .Get<string[]>() ?? Array.Empty<string>();

        foreach (var path in knowledgePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            try
            {
                var resolvedPath = ResolveKnowledgePath(path);
                if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
                {
                    continue;
                }

                var text = ReadKnowledgeFile(resolvedPath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

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
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var localPath = Path.Combine(_environment.ContentRootPath, configuredPath);
        if (File.Exists(localPath))
        {
            return localPath;
        }

        var projectRootCandidate = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, ".."));
        var rootPath = Path.Combine(projectRootCandidate, configuredPath);
        if (File.Exists(rootPath))
        {
            return rootPath;
        }

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
}
