public class TransactionSummary
{
    public decimal TotalCredits { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AverageTransaction { get; set; }
    public int TransactionCount { get; set; }
    public int UncategorizedCount { get; set; }
}
