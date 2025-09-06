using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Services
{
    public interface ITransactionService
    {
        Task<CsvImportResult> ImportCsvAsync(IFormFile file, string userId);
        Task<PagedResponse<Transaction>> GetTransactionsAsync(GetTransactions request, string userId);
        Task<TransactionSummary> GetTransactionSummaryAsync(string userId, bool excludeInternalTransfers = false);
        Task<Transaction?> GetTransactionByIdAsync(int id, string userId);
        Task UpdateTransactionCategoryAsync(int transactionId, int categoryId, string userId);
    }
}
