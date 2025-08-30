namespace PersonalFinanceTracker.DTO
{
    public class CategoryDeleteResult
    {
        public string Message { get; set; } = string.Empty;
        public int UncategorizedCount { get; set; }
    }

}
