using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Persistence
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id, string userId);
        Task<Transaction?> GetByHashAsync(string hash, string userId);
        Task<List<Transaction>> GetAllAsync(string userId);
        Task<List<Transaction>> GetByCategoryIdAsync(int categoryId, string userId);
        Task<PagedResponse<Transaction>> GetPagedAsync(
            string userId,
            int page, 
            int pageSize, 
            string? search = null, 
            string sortField = "date", 
            string sortDirection = "desc", 
            string? category = null);
        Task<List<Transaction>> GetTransactionsByFiltersAsync(
            string userId,
            string? descriptionPattern = null,
            decimal? amountMin = null,
            decimal? amountMax = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);
        Task<List<Transaction>> GetSimilarUncategorizedTransactionsAsync(string businessName, string userId);
        Task<int> GetCountAsync(string userId);
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task UpdateRangeAsync(IEnumerable<Transaction> transactions);
        Task SaveChangesAsync();
    }
}
