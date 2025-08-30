using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Persistence
{
    public class TransactionDbContext : DbContext
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure decimal precision for money fields
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Credit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Balance)
                .HasPrecision(18, 2);

            // Add index for better query performance
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Date);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Category);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.CategoryNavigation)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed default categories
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Uncategorized", Color = "#808080" },  // Gray
                new Category { Id = 2, Name = "Food & Dining", Color = "#FF5733" },  // Orange-Red
                new Category { Id = 3, Name = "Transport", Color = "#33FFF5" },      // Cyan
                new Category { Id = 4, Name = "Shopping", Color = "#3357FF" },       // Blue
                new Category { Id = 5, Name = "Bills & Utilities", Color = "#FF33F5" }, // Pink
                new Category { Id = 6, Name = "Income", Color = "#33FF57" },         // Green
                new Category { Id = 7, Name = "Groceries", Color = "#3F502F" },         // Dark green
                new Category { Id = 8, Name = "Coffee", Color = "#A0522D" },         // Brown
                new Category { Id = 9, Name = "Petrol", Color = "#FFD700" },         // Gold/Yellow
                new Category { Id = 10, Name = "Transfers", Color = "#8A2BE2" },      // Purple
                new Category { Id = 11, Name = "Fitness", Color = "#FF4500" },       // Orange
                new Category { Id = 13, Name = "Car", Color = "#FFB2AA" },        // Salmon
                new Category { Id = 14, Name = "Gifts", Color = "#CB22AA" },        // Deep pink
                new Category { Id = 15, Name = "Rebates", Color = "#E1EAAA" },        // Crap green
                new Category { Id = 16, Name = "Leisure", Color = "#A1EAAA" }        // Soft green
            );
        }

    }

}
