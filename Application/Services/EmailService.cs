using System.Text;
using System.Text.Json;

namespace PersonalFinanceTracker.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetUrl)
        {
            var apiKey = _configuration["Resend:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine($"=== PASSWORD RESET REQUEST ===");
                Console.WriteLine($"Email: {email}");
                Console.WriteLine($"Reset URL: {resetUrl}");
                Console.WriteLine($"===============================");
                return;
            }

            var emailData = new
            {
                from = "personal-finance-tracker@resend.dev",
                to = new[] { email },
                subject = "Reset Your Password",
                html = $@"
                    <h2>Reset Your Password</h2>
                    <p>You requested a password reset for your Personal Finance Tracker account.</p>
                    <p><a href='{resetUrl}' style='background-color: #3B82F6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                    <p>If you didn't request this, you can safely ignore this email.</p>
                    <p>This link will expire in 1 hour.</p>
                "
            };

            var json = JsonSerializer.Serialize(emailData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send email: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
