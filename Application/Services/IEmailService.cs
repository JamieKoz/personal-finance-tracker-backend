namespace PersonalFinanceTracker.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetUrl);
    }
}
