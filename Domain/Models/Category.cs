using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(7)]
        public string? Color { get; set; } = "#6B7280";

        public DateTime CreatedAt { get; set; }

        public List<Transaction> Transactions { get; set; } = new();
    }
}
