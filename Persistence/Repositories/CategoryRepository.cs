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

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<List<CategoryWithCount>> GetCategoriesWithTransactionCountAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryWithCount
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    TransactionCount = c.Transactions.Count()
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
