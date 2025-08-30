using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Persistence;

namespace PersonalFinanceTracker.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddDbContext<TransactionDbContext>(options => options.UseSqlite("Data Source=transactions.db"));
            return services;
        }
    }
}
