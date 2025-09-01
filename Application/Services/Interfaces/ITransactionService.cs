using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Services
{
    public interface ITransactionService
    {
        Task<CsvImportResult> ImportCsvAsync(IFormFile file);
        Task<PagedResponse<Transaction>> GetTransactionsAsync(GetTransactions request);
        Task<TransactionSummary> GetTransactionSummaryAsync(bool excludeInternalTransfers = false);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task UpdateTransactionCategoryAsync(int transactionId, int categoryId);
    }

}
