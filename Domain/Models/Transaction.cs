using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Credit { get; set; }

        [Required]
        [MaxLength(100)]
        public decimal Balance { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = "Uncategorized";

        [MaxLength(100)]
        public string? ImportHash { get; set; }

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        public Category? CategoryNavigation { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
