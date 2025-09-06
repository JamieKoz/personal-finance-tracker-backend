using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Persistence
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id, string userId);
        Task<Category?> GetByNameAsync(string name, string userId);
        Task<List<CategoryWithCount>> GetCategoriesWithTransactionCountAsync(string userId);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task SaveChangesAsync();
    }
}
