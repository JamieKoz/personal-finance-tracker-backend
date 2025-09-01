namespace PersonalFinanceTracker.DTO
{
    public class CategorizeResult
    {
        public string Message { get; set; } = string.Empty;
        public int UpdatedCount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
    }
}
