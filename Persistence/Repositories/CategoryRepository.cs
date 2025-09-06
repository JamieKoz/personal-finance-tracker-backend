using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Persistence
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly TransactionDbContext _context;

        public CategoryRepository(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id, string userId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<Category?> GetByNameAsync(string name, string userId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && c.UserId == userId);
        }

        public async Task<List<CategoryWithCount>> GetCategoriesWithTransactionCountAsync(string userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .Select(c => new CategoryWithCount
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    TransactionCount = c.Transactions.Count(t => t.UserId == userId)
                })
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
