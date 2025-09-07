namespace PersonalFinanceTracker.Api
{
    public static class ApiRegistration
    {

        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenApi();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins(
                        "https://personal-finance-tracker-frontend.pages.dev",     // For Cloudflare Pages
                        "http://localhost:3000"  // For local development
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });
            return services;
        }
    }
}
