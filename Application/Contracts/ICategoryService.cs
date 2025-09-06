using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<object>> GetCategoriesAsync(string userId);
        Task<Category> CreateCategoryAsync(CreateCategory request, string userId);
        Task<Category> UpdateCategoryAsync(int id, UpdateCategory request, string userId);
        Task<CategoryDeleteResult> DeleteCategoryAsync(int id, string userId);
        Task<CategorizeResult> CategorizeTransactionsAsync(Categorize request, string userId);
        Task<CategorizeResult> CategorizeWithPatternAsync(CategorizeWithPattern request, string userId);
    }
}
