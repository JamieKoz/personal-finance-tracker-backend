using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace PersonalFinanceTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly TransactionDbContext _context;

        public CategoryController(TransactionDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Color,
                    c.CreatedAt,
                    TransactionCount = c.Transactions.Count  // Change this line
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategory request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                Color = request.Color ?? "#6B7280"
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategory request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            category.Name = request.Name;
            category.Description = request.Description;
            category.Color = request.Color ?? category.Color;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            // Set all transactions with this category to uncategorized
            var transactions = await _context.Transactions
                .Where(t => t.CategoryId == id)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.CategoryId = null;
                transaction.Category = "Uncategorized";
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Category deleted and {transactions.Count} transactions set to uncategorized" });
        }

        [HttpPost("categorize")]
        public async Task<IActionResult> CategorizeTransactions([FromBody] Categorize request)
        {
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest("Category not found");

            var query = _context.Transactions.AsQueryable();

            // Apply pattern matching
            if (!string.IsNullOrEmpty(request.DescriptionPattern))
            {
                query = query.Where(t => t.Description.Contains(request.DescriptionPattern));
            }

            if (request.AmountMin.HasValue)
            {
                query = query.Where(t => t.Credit >= request.AmountMin.Value);
            }

            if (request.AmountMax.HasValue)
            {
                query = query.Where(t => t.Credit <= request.AmountMax.Value);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(t => t.Date >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(t => t.Date <= request.DateTo.Value);
            }

            var matchingTransactions = await query.ToListAsync();

            // Update the matching transactions
            foreach (var transaction in matchingTransactions)
            {
                transaction.CategoryId = category.Id;
                transaction.Category = category.Name;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Successfully categorized {matchingTransactions.Count} transactions as '{category.Name}'",
                UpdatedCount = matchingTransactions.Count,
                Category = category.Name
            });
        }

        [HttpPost("categorize-with-pattern")]
        public async Task<IActionResult> CategorizeWithPattern([FromBody] CategorizeWithPattern request)
        {
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest("Category not found");

            var baseTransaction = await _context.Transactions.FindAsync(request.TransactionId);
            if (baseTransaction == null)
                return BadRequest("Transaction not found");

            // Check if this is a transfer - if so, only categorize this specific transaction
            if (baseTransaction.Description.ToLower().Contains("transfer"))
            {
                baseTransaction.CategoryId = category.Id;
                baseTransaction.Category = category.Name;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = $"Categorized 1 transfer transaction as '{category.Name}' (no pattern matching applied)",
                    UpdatedCount = 1,
                    BusinessName = "N/A - Transfer",
                    Category = category.Name
                });
            }

            // Extract business name from description
            string businessName = BusinessNameExtractor.ExtractBusinessName(baseTransaction.Description);

            // If no meaningful business name extracted, categorize only this transaction
            if (string.IsNullOrEmpty(businessName))
            {
                baseTransaction.CategoryId = category.Id;
                baseTransaction.Category = category.Name;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = $"Categorized 1 transaction as '{category.Name}' (no pattern available)",
                    UpdatedCount = 1,
                    BusinessName = "N/A",
                    Category = category.Name
                });
            }

            // Find all transactions with similar business names, excluding transfers
            // Using ToLower() instead of StringComparison for EF compatibility
            var similarTransactions = await _context.Transactions
                .Where(t => t.Description.Contains(businessName) &&
                           (t.CategoryId == null || t.Category == "Uncategorized") &&
                           !t.Description.ToLower().Contains("transfer"))
                .ToListAsync();

            // Update all similar transactions
            foreach (var transaction in similarTransactions)
            {
                transaction.CategoryId = category.Id;
                transaction.Category = category.Name;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Successfully categorized {similarTransactions.Count} transactions as '{category.Name}' using pattern '{businessName}'",
                UpdatedCount = similarTransactions.Count,
                BusinessName = businessName,
                Category = category.Name
            });
        }

    }
}
