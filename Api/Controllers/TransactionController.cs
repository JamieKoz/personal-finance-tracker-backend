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
    public class TransactionController : ControllerBase
    {
        private readonly CsvParser _csvParser;
        private readonly TransactionDbContext _context;

        public TransactionController(TransactionDbContext context)
        {
            _csvParser = new CsvParser();
            _context = context;
        }

        // @TODO Add encryption for file upload
        [HttpPost("upload-csv")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var csvContent = await reader.ReadToEndAsync();
                var parsedTransactions = _csvParser.ParseCsv(csvContent);

                int newTransactionsCount = 0;
                int duplicatesSkipped = 0;

                foreach (var transaction in parsedTransactions)
                {
                    // Generate hash for duplicate detection
                    transaction.ImportHash = GenerateTransactionHash(transaction);

                    // Check if this transaction already exists
                    var existingTransaction = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.ImportHash == transaction.ImportHash);

                    if (existingTransaction == null)
                    {
                        _context.Transactions.Add(transaction);
                        newTransactionsCount++;
                    }
                    else
                    {
                        duplicatesSkipped++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = $"Import completed successfully",
                    NewTransactions = newTransactionsCount,
                    DuplicatesSkipped = duplicatesSkipped,
                    TotalTransactionsInDatabase = await _context.Transactions.CountAsync()
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing CSV: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllTransactions(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] string? search = null,
    [FromQuery] string? sortField = "date",
    [FromQuery] string? sortDirection = "desc",
    [FromQuery] string? category = null)
        {
            pageSize = Math.Min(pageSize, 100); // Cap at 100
            page = Math.Max(page, 1);

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

            // Apply sorting across all data
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

            var response = new PagedResponse<Transaction>
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

            return Ok(response);
        }

        
[HttpGet("summary")]
public async Task<IActionResult> GetTransactionSummary([FromQuery] bool excludeInternalTransfers = false)
{
    var allTransactions = await _context.Transactions.ToListAsync();
    if (!allTransactions.Any())
    {
        return Ok(new TransactionSummary
        {
            TotalCredits = 0,
            TotalDebits = 0,
            CurrentBalance = 0,
            AverageTransaction = 0,
            TransactionCount = 0,
            UncategorizedCount = 0,
            TotalSavings = 0,
            SavingsRate = 0,
            ActualSpending = 0,
            ActualIncome = 0
        });
    }

    var filteredTransactions = allTransactions;
    
    if (excludeInternalTransfers)
    {
        // Filter out internal transfers based on common patterns
        filteredTransactions = allTransactions.Where(t => 
            !IsInternalTransfer(t.Description, t.Category)
        ).ToList();
    }

    var totalCredits = filteredTransactions.Where(t => t.Credit > 0).Sum(t => t.Credit);
    var totalDebits = Math.Abs(filteredTransactions.Where(t => t.Credit < 0).Sum(t => t.Credit));
    var currentBalance = allTransactions.OrderByDescending(t => t.Date).First().Balance;
    var averageTransaction = filteredTransactions.Any() ? filteredTransactions.Average(t => Math.Abs(t.Credit)) : 0;

    // Calculate savings and spending
    var actualIncome = totalCredits;
    var actualSpending = totalDebits;
    var totalSavings = actualIncome - actualSpending;
    var savingsRate = actualIncome > 0 ? (totalSavings / actualIncome) * 100 : 0;

    // Count uncategorized transactions
    var uncategorizedCount = allTransactions.Count(t =>
        t.CategoryId == null ||
        string.IsNullOrEmpty(t.Category) ||
        t.Category == "Uncategorized"
    );

    var summary = new TransactionSummary
    {
        TotalCredits = totalCredits,
        TotalDebits = totalDebits,
        CurrentBalance = currentBalance,
        AverageTransaction = averageTransaction,
        TransactionCount = filteredTransactions.Count,
        UncategorizedCount = uncategorizedCount,
        TotalSavings = totalSavings,
        SavingsRate = Math.Round(savingsRate, 2),
        ActualSpending = actualSpending,
        ActualIncome = actualIncome,
        InternalTransfersExcluded = excludeInternalTransfers,
        OriginalTransactionCount = allTransactions.Count
    };

    return Ok(summary);
}

private bool IsInternalTransfer(string description, string category)
{
    var desc = description?.ToLower() ?? "";
    var cat = category?.ToLower() ?? "";
    
    // Common internal transfer patterns
    var internalPatterns = new[]
    {
        "transfer", "tfr", "internal", "savings", "investment",
        "credit card payment", "cc payment", "loan payment",
        "mortgage payment", "between accounts", "account transfer",
        "online transfer", "mobile transfer", "wire transfer",
        "deposit to", "withdrawal from", "move money"
    };
    
    var internalCategories = new[]
    {
        "transfers", "internal transfer", "savings", "investments",
        "loan payments", "credit card payments", "account transfers"
    };
    
    return internalPatterns.Any(pattern => desc.Contains(pattern)) ||
           internalCategories.Any(pattern => cat.Contains(pattern));
}


        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            
            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpPut("{id}/category")]
        public async Task<IActionResult> UpdateTransactionCategory(int id, [FromBody] UpdateTransaction request)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return BadRequest("Category not found");

            transaction.CategoryId = category.Id;
            transaction.Category = category.Name;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Transaction category updated successfully" });
        }

        private string GenerateTransactionHash(Transaction transaction)
        {
            var input = $"{transaction.Date:yyyy-MM-dd}|{transaction.Description}|{transaction.Credit}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }
    }
}
