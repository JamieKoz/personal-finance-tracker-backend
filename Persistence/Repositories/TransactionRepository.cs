using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;
using PersonalFinanceTracker.Persistence;
using PersonalFinanceTracker.ValueObjects;

namespace PersonalFinanceTracker.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TransactionDbContext _context;

        public TransactionRepository(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<Transaction?> GetByHashAsync(string hash)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.ImportHash == hash);
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task<List<Transaction>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Transactions
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<PagedResponse<Transaction>> GetPagedAsync(
            int page, 
            int pageSize, 
            string? search = null, 
            string sortField = "date", 
            string sortDirection = "desc", 
            string? category = null)
        {
            var query = _context.Transactions.AsQueryable();

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category == category);
            }

            // Apply fuzzy search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower().Trim();
                query = query.Where(t =>
                    EF.Functions.Like(t.Description.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(t.Category != null ? t.Category.ToLower() : "", $"%{searchTerm}%"));
            }

            // Apply sorting
            query = sortField?.ToLower() switch
            {
                "date" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Date)
                    : query.OrderByDescending(t => t.Date),
                "description" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Description)
                    : query.OrderByDescending(t => t.Description),
                "category" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Category)
                    : query.OrderByDescending(t => t.Category),
                "credit" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Credit)
                    : query.OrderByDescending(t => t.Credit),
                "balance" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Balance)
                    : query.OrderByDescending(t => t.Balance),
                _ => query.OrderByDescending(t => t.Date)
            };

            var totalCount = await query.CountAsync();
            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Transaction>
            {
                Data = transactions,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = page * pageSize < totalCount,
                    HasPreviousPage = page > 1
                }
            };
        }

        public async Task<List<Transaction>> GetTransactionsByFiltersAsync(
            string? descriptionPattern = null,
            decimal? amountMin = null,
            decimal? amountMax = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.Transactions.AsQueryable();

            // Apply pattern matching
            if (!string.IsNullOrEmpty(descriptionPattern))
            {
                query = query.Where(t => t.Description.Contains(descriptionPattern));
            }

            if (amountMin.HasValue)
            {
                query = query.Where(t => t.Credit >= amountMin.Value);
            }

            if (amountMax.HasValue)
            {
                query = query.Where(t => t.Credit <= amountMax.Value);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(t => t.Date >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(t => t.Date <= dateTo.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<Transaction>> GetSimilarUncategorizedTransactionsAsync(string businessName)
        {
            return await _context.Transactions
                .Where(t => t.Description.Contains(businessName) &&
                           (t.CategoryId == null || t.Category == "Uncategorized") &&
                           !t.Description.ToLower().Contains("transfer"))
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Transactions.CountAsync();
        }

        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Transaction> transactions)
        {
            _context.Transactions.UpdateRange(transactions);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
