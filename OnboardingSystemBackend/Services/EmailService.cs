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

    private async Task SendEmailAsync(string toAddress, string subject, string body)
    {
        try
        {
            var host = _configuration["Smtp:Host"] ?? "localhost";
            var portString = _configuration["Smtp:Port"] ?? "1025";
            if (!int.TryParse(portString, out int port))
            {
                port = 1025;
            }

            using var client = new SmtpClient(host, port);
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@onboarding.local", "Система Онбординга"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toAddress);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent to {EmailAddress} with subject '{Subject}'", toAddress, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress}", toAddress);
            // We don't want email sending failure to crash the main business flow usually
        }
    }
}
