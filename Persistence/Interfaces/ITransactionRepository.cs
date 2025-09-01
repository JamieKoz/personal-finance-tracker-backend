using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Persistence
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction?> GetByHashAsync(string hash);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId);
        Task<PagedResponse<Transaction>> GetPagedAsync(
            int page, 
            int pageSize, 
            string? search = null, 
            string sortField = "date", 
            string sortDirection = "desc", 
            string? category = null);
        Task<IEnumerable<Transaction>> GetTransactionsByFiltersAsync(
            string? descriptionPattern = null,
            decimal? amountMin = null,
            decimal? amountMax = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);
        Task<IEnumerable<Transaction>> GetSimilarUncategorizedTransactionsAsync(string businessName);
        Task<int> GetCountAsync();
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task UpdateRangeAsync(IEnumerable<Transaction> transactions);
        Task SaveChangesAsync();
    }
}
