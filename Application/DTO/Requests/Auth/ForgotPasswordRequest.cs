using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.DTO.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
