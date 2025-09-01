namespace PersonalFinanceTracker.DTO
{
    public class CsvImportResult
    {
        public string Message { get; set; } = string.Empty;
        public int NewTransactions { get; set; }
        public int DuplicatesSkipped { get; set; }
        public int TotalTransactionsInDatabase { get; set; }
    }
}
