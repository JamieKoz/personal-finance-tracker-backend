using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Persistence
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction?> GetByHashAsync(string hash);
        Task<List<Transaction>> GetAllAsync();
        Task<List<Transaction>> GetByCategoryIdAsync(int categoryId);
        Task<PagedResponse<Transaction>> GetPagedAsync(
            int page, 
            int pageSize, 
            string? search = null, 
            string sortField = "date", 
            string sortDirection = "desc", 
            string? category = null);
        Task<List<Transaction>> GetTransactionsByFiltersAsync(
            string? descriptionPattern = null,
            decimal? amountMin = null,
            decimal? amountMax = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);
        Task<List<Transaction>> GetSimilarUncategorizedTransactionsAsync(string businessName);
        Task<int> GetCountAsync();
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task UpdateRangeAsync(IEnumerable<Transaction> transactions);
        Task SaveChangesAsync();
    }
}
