namespace OnboardingSystem.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string fullName);
    Task SendOnboardingCompletedEmailAsync(string hrEmail, string mentorEmail, string employeeName);
    Task SendPasswordSetupEmailAsync(string toEmail, string fullName, string setupUrl);
}
