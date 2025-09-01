using Microsoft.AspNetCore.Http;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<object>> GetCategoriesAsync();
        Task<Category> CreateCategoryAsync(CreateCategory request);
        Task<Category> UpdateCategoryAsync(int id, UpdateCategory request);
        Task<CategoryDeleteResult> DeleteCategoryAsync(int id);
        Task<CategorizeResult> CategorizeTransactionsAsync(Categorize request);
        Task<CategorizeResult> CategorizeWithPatternAsync(CategorizeWithPattern request);
    }
}
