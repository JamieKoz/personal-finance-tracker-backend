using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Persistence
{
    public class TransactionDbContext : IdentityDbContext<ApplicationUser>
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Credit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Date);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Category);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.UserId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.CategoryNavigation)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // NEW: Configure user relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }

        // Helper method to get default categories for a new user
        public static List<Category> GetDefaultCategoriesForUser(string userId)
        {
            return new List<Category>
            {
                new Category { Name = "Uncategorized", Color = "#808080", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Food & Dining", Color = "#FF5733", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Transport", Color = "#33FFF5", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Shopping", Color = "#3357FF", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Bills & Utilities", Color = "#FF33F5", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Income", Color = "#33FF57", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Groceries", Color = "#3F502F", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Coffee", Color = "#A0522D", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Petrol", Color = "#FFD700", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Transfers", Color = "#8A2BE2", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Fitness", Color = "#FF4500", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Car", Color = "#FFB2AA", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Gifts", Color = "#CB22AA", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Rebates", Color = "#E1EAAA", UserId = userId, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Leisure", Color = "#A1EAAA", UserId = userId, CreatedAt = DateTime.UtcNow }
            };
        }
    }
}
