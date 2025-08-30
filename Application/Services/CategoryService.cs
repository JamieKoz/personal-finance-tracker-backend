using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Persistence;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ITransactionRepository transactionRepository)
        {
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<object>> GetCategoriesAsync(string userId)
        {
            var categories = await _categoryRepository.GetCategoriesWithTransactionCountAsync(userId);
            return categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Color,
                c.CreatedAt,
                c.TransactionCount
            }).OrderBy(c => c.Name);
        }

        public async Task<Category> CreateCategoryAsync(CreateCategory request, string userId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required");

            // Check if category with same name already exists for this user
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, userId);
            if (existingCategory != null)
                throw new ArgumentException($"Category with name '{request.Name}' already exists");

            var category = new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Color = request.Color ?? "#6B7280",
                UserId = userId
            };

            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(int id, UpdateCategory request, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(id, userId);
            if (category == null)
                throw new ArgumentException("Category not found");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category name is required");

            // Check if another category with same name already exists for this user (excluding current category)
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, userId);
            if (existingCategory != null && existingCategory.Id != id)
                throw new ArgumentException($"Category with name '{request.Name}' already exists");

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();
            category.Color = request.Color ?? category.Color;

            await _categoryRepository.UpdateAsync(category);
            return category;
        }

        public async Task<CategoryDeleteResult> DeleteCategoryAsync(int id, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(id, userId);
            if (category == null) {
                throw new ArgumentException("Category not found");
            }

            // Get all transactions with this category for this user
            var transactions = await _transactionRepository.GetByCategoryIdAsync(id, userId);

            // Set all transactions with this category to uncategorized
            foreach (var transaction in transactions)
            {
                transaction.CategoryId = null;
                transaction.Category = "Uncategorized";
            }

            if (transactions.Any())
            {
                await _transactionRepository.UpdateRangeAsync(transactions);
            }

            await _categoryRepository.DeleteAsync(category);

            return new CategoryDeleteResult
            {
                Message = $"Category deleted and {transactions.Count()} transactions set to uncategorized",
                UncategorizedCount = transactions.Count()
            };
        }

        public async Task<CategorizeResult> CategorizeTransactionsAsync(Categorize request, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId);
            if (category == null) {
                throw new ArgumentException("Category not found");
            }

            var matchingTransactions = await _transactionRepository.GetTransactionsByFiltersAsync(
                userId: userId,
                descriptionPattern: request.DescriptionPattern,
                amountMin: request.AmountMin,
                amountMax: request.AmountMax,
                dateFrom: request.DateFrom,
                dateTo: request.DateTo
            );

            // Update the matching transactions
            foreach (var transaction in matchingTransactions)
            {
                transaction.CategoryId = category.Id;
                transaction.Category = category.Name;
            }

            if (matchingTransactions.Any())
            {
                await _transactionRepository.UpdateRangeAsync(matchingTransactions);
            }

            return new CategorizeResult
            {
                Message = $"Successfully categorized {matchingTransactions.Count()} transactions as '{category.Name}'",
                UpdatedCount = matchingTransactions.Count(),
                Category = category.Name
            };
        }

        public async Task<CategorizeResult> CategorizeWithPatternAsync(CategorizeWithPattern request, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, userId);
            if (category == null) {
                throw new ArgumentException("Category not found");
            }

            var baseTransaction = await _transactionRepository.GetByIdAsync(request.TransactionId, userId);
            if (baseTransaction == null) {
                throw new ArgumentException("Transaction not found");
            }

            // Check if this is a transfer - if so, only categorize this specific transaction
            if (IsTransfer(baseTransaction.Description))
            {
                baseTransaction.CategoryId = category.Id;
                baseTransaction.Category = category.Name;
                await _transactionRepository.UpdateAsync(baseTransaction);

                return new CategorizeResult
                {
                    Message = $"Categorized 1 transfer transaction as '{category.Name}' (no pattern matching applied)",
                    UpdatedCount = 1,
                    BusinessName = "N/A - Transfer",
                    Category = category.Name
                };
            }

            // Extract business name from description
            string businessName = BusinessNameExtractor.ExtractBusinessName(baseTransaction.Description);

            // If no meaningful business name extracted, categorize only this transaction
            if (string.IsNullOrEmpty(businessName))
            {
                baseTransaction.CategoryId = category.Id;
                baseTransaction.Category = category.Name;
                await _transactionRepository.UpdateAsync(baseTransaction);

                return new CategorizeResult
                {
                    Message = $"Categorized 1 transaction as '{category.Name}' (no pattern available)",
                    UpdatedCount = 1,
                    BusinessName = "N/A",
                    Category = category.Name
                };
            }

            // Find all uncategorized transactions with similar business names for this user, excluding transfers
            var similarTransactions = await _transactionRepository.GetSimilarUncategorizedTransactionsAsync(businessName, userId);

            // Update all similar transactions
            foreach (var transaction in similarTransactions)
            {
                transaction.CategoryId = category.Id;
                transaction.Category = category.Name;
            }

            if (similarTransactions.Any())
            {
                await _transactionRepository.UpdateRangeAsync(similarTransactions);
            }

            return new CategorizeResult
            {
                Message = $"Successfully categorized {similarTransactions.Count()} transactions as '{category.Name}' using pattern '{businessName}'",
                UpdatedCount = similarTransactions.Count(),
                BusinessName = businessName,
                Category = category.Name
            };
        }

        private static bool IsTransfer(string description)
        {
            return description?.ToLower().Contains("transfer") ?? false;
        }
    }
}
