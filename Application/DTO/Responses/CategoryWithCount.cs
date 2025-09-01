namespace PersonalFinanceTracker.DTO
{
    public class CategoryWithCount
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TransactionCount { get; set; }
    }
}
