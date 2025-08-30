using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;
using PersonalFinanceTracker.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace PersonalFinanceTracker.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly CsvParser _csvParser;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            CsvParser csvParser)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _csvParser = csvParser;
        }

        public async Task<CsvImportResult> ImportCsvAsync(IFormFile file, string userId)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var csvContent = await reader.ReadToEndAsync();
            var parsedTransactions = _csvParser.ParseCsv(csvContent);

            int newTransactionsCount = 0;
            int duplicatesSkipped = 0;

            // Get user's categories for auto-categorization
            var userCategories = await _categoryRepository.GetCategoriesWithTransactionCountAsync(userId);
            var transfersCategory = userCategories.FirstOrDefault(c => c.Name.ToLower() == "transfers");

            foreach (var transaction in parsedTransactions)
            {
                transaction.UserId = userId;
                transaction.ImportHash = GenerateTransactionHash(transaction);

                var existingTransaction = await _transactionRepository.GetByHashAsync(transaction.ImportHash, userId);

                if (existingTransaction != null)
                {
                    duplicatesSkipped++;
                    continue;
                }

                // Auto-categorize transfers
                if (IsInternalTransfer(transaction.Description, transaction.Category))
                {
                    if (transfersCategory != null)
                    {
                        transaction.CategoryId = transfersCategory.Id;
                        transaction.Category = "Transfers";
                    }
                }

                await _transactionRepository.AddAsync(transaction);
                newTransactionsCount++;
            }

            await _transactionRepository.SaveChangesAsync();
            var totalCount = await _transactionRepository.GetCountAsync(userId);

            return new CsvImportResult
            {
                Message = "Import completed successfully",
                NewTransactions = newTransactionsCount,
                DuplicatesSkipped = duplicatesSkipped,
                TotalTransactionsInDatabase = totalCount
            };
        }

        // Rest of the existing methods remain the same...
        public async Task<PagedResponse<Transaction>> GetTransactionsAsync(GetTransactions request, string userId)
        {
            request.PageSize = Math.Min(request.PageSize, 100);
            request.Page = Math.Max(request.Page, 1);

            var result = await _transactionRepository.GetPagedAsync(
                userId: userId,
                page: request.Page,
                pageSize: request.PageSize,
                search: request.Search,
                sortField: request.SortField ?? "date",
                sortDirection: request.SortDirection ?? "desc",
                category: request.Category
            );

            return result;
        }

        public async Task<TransactionSummary> GetTransactionSummaryAsync(string userId, bool excludeInternalTransfers = false)
        {
            var allTransactions = await _transactionRepository.GetAllAsync(userId);
            
            if (!allTransactions.Any())
            {
                return CreateEmptySummary();
            }

            var filteredTransactions = allTransactions;

            if (excludeInternalTransfers)
            {
                filteredTransactions = allTransactions.Where(t =>
                    !IsInternalTransfer(t.Description, t.Category)
                ).ToList();
            }

            return CalculateTransactionSummary(allTransactions, filteredTransactions, excludeInternalTransfers);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id, string userId)
        {
            return await _transactionRepository.GetByIdAsync(id, userId);
        }

        public async Task UpdateTransactionCategoryAsync(int transactionId, int categoryId, string userId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, userId);
            if (transaction == null) {
                throw new ArgumentException("Transaction not found");
            }

            var category = await _categoryRepository.GetByIdAsync(categoryId, userId);
            if (category == null) {
                throw new InvalidOperationException("Category not found");
            }

            transaction.CategoryId = category.Id;
            transaction.Category = category.Name;

            await _transactionRepository.UpdateAsync(transaction);
        }

        private string GenerateTransactionHash(Transaction transaction)
        {
            var input = $"{transaction.Date:yyyy-MM-dd}|{transaction.Description}|{transaction.Credit}|{transaction.UserId}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        private bool IsInternalTransfer(string description, string category)
        {
            var desc = description?.ToLower() ?? "";
            var cat = category?.ToLower() ?? "";

            var internalPatterns = new[]
            {
                "internal transfer", 
            };

            var internalCategories = new[]
            {
                "internal transfer",
            };

            return internalPatterns.Any(pattern => desc.Contains(pattern)) ||
                   internalCategories.Any(pattern => cat.Contains(pattern));
        }

        private TransactionSummary CreateEmptySummary()
        {
            return new TransactionSummary
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
            };
        }

        private TransactionSummary CalculateTransactionSummary(
            IEnumerable<Transaction> allTransactions, 
            IEnumerable<Transaction> filteredTransactions, 
            bool excludeInternalTransfers)
        {
            var totalCredits = filteredTransactions.Where(t => t.Credit > 0).Sum(t => t.Credit);
            var totalDebits = Math.Abs(filteredTransactions.Where(t => t.Credit < 0).Sum(t => t.Credit));
            var currentBalance = allTransactions.OrderByDescending(t => t.Date).First().Balance;
            var averageTransaction = filteredTransactions.Any() ? filteredTransactions.Average(t => Math.Abs(t.Credit)) : 0;

            var actualIncome = totalCredits;
            var actualSpending = totalDebits;
            var totalSavings = actualIncome - actualSpending;
            var savingsRate = actualIncome > 0 ? (totalSavings / actualIncome) * 100 : 0;

            var uncategorizedCount = allTransactions.Count(t =>
                t.CategoryId == null ||
                string.IsNullOrEmpty(t.Category) ||
                t.Category == "Uncategorized"
            );

            return new TransactionSummary
            {
                TotalCredits = totalCredits,
                TotalDebits = totalDebits,
                CurrentBalance = currentBalance,
                AverageTransaction = averageTransaction,
                TransactionCount = filteredTransactions.Count(),
                UncategorizedCount = uncategorizedCount,
                TotalSavings = totalSavings,
                SavingsRate = Math.Round(savingsRate, 2),
                ActualSpending = actualSpending,
                ActualIncome = actualIncome,
                InternalTransfersExcluded = excludeInternalTransfers,
                OriginalTransactionCount = allTransactions.Count()
            };
        }
    }
}
