namespace PersonalFinanceTracker.DTO
{
    public class TransactionSummary
    {
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AverageTransaction { get; set; }
        public int TransactionCount { get; set; }
        public int UncategorizedCount { get; set; }

        // New properties for savings analysis
        public decimal TotalSavings { get; set; }
        public decimal SavingsRate { get; set; }
        public decimal ActualSpending { get; set; }
        public decimal ActualIncome { get; set; }
        public bool InternalTransfersExcluded { get; set; }
        public int OriginalTransactionCount { get; set; }
    }
}
