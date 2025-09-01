namespace PersonalFinanceTracker.DTO
{
    public class GetTransactions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? Search { get; set; }
        public string? SortField { get; set; } = "date";
        public string? SortDirection { get; set; } = "desc";
        public string? Category { get; set; }
    }
}
