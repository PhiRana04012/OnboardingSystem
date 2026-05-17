using System.Net.Mail;
using System.Net;

namespace OnboardingSystem.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string fullName)
    {
        var subject = "Добро пожаловать в компанию!";
        var body = $"<h3>Уважаемый(ая) {fullName}!</h3>" +
                   $"<p>Рады приветствовать вас в нашей команде.</p>" +
                   $"<p>Вам предоставлен доступ к Системе онбординга. Вы можете войти, используя свой рабочий email ({toEmail}).</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendOnboardingCompletedEmailAsync(string hrEmail, string mentorEmail, string employeeName)
    {
        var subject = $"Окончание онбординга: {employeeName}";
        var body = $"<h3>Уведомление</h3>" +
                   $"<p>Сотрудник <strong>{employeeName}</strong> успешно завершил(а) программу онбординга.</p>";

        var recipients = new List<string>();
        if (!string.IsNullOrEmpty(hrEmail)) recipients.Add(hrEmail);
        if (!string.IsNullOrEmpty(mentorEmail)) recipients.Add(mentorEmail);

        foreach (var recipient in recipients)
        {
            await SendEmailAsync(recipient, subject, body);
        }
    }

    public async Task SendPasswordSetupEmailAsync(string toEmail, string fullName, string setupUrl)
    {
        var subject = "Установите пароль для входа в систему онбординга";
        var body = $@"
            <h3>Добро пожаловать, {fullName}!</h3>
            <p>Вам создана учетная запись в Системе онбординга.</p>
            <p>Чтобы начать работу, пожалуйста, <strong>установите свой пароль</strong>, перейдя по ссылке ниже:</p>
            <p>
                <a href=""{setupUrl}"" style=""background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;"">
                    Установить пароль
                </a>
            </p>
            <p><strong>Важно:</strong> Ссылка действительна в течение 24 часов.</p>
            <p>Если кнопка выше не работает, скопируйте и вставьте эту ссылку в адресную строку браузера:</p>
            <p><code>{setupUrl}</code></p>
            <p>Если вы не регистрировались в нашей системе, пожалуйста, проигнорируйте это письмо.</p>
            <br/>
            <p>С уважением,<br/>Команда Системы онбординга</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toAddress, string subject, string body)
    {
        try
        {
            var host = _configuration["Smtp:Host"] ?? "localhost";
            var portString = _configuration["Smtp:Port"] ?? "1025";
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var enableSslString = _configuration["Smtp:EnableSsl"] ?? "false";
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@onboarding.local";
            var fromName = _configuration["Smtp:FromName"] ?? "Система Онбординга";

            if (!int.TryParse(portString, out int port))
            {
                port = 1025;
            }

            if (!bool.TryParse(enableSslString, out bool enableSsl))
            {
                enableSsl = false;
            }

            _logger.LogInformation($"📧 SMTP Config: Host={host}, Port={port}, Username={username}, SSL={enableSsl}");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            // Если есть учетные данные, используем аутентификацию
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                _logger.LogInformation($"🔐 Using SMTP authentication for user: {username}");
                client.Credentials = new NetworkCredential(username, password);
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toAddress);

            _logger.LogInformation($"📤 Sending email to {toAddress}...");
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"✅ Email sent to {toAddress} with subject '{subject}' from {fromEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Failed to send email to {toAddress}. Error: {ex.Message}. StackTrace: {ex.StackTrace}");
        }
    }
}
