using PersonalFinanceTracker.Persistence;
using PersonalFinanceTracker.Application;
using PersonalFinanceTracker.Api;
using PersonalFinanceTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddSingleton<SqliteBackupService>();

var app = builder.Build();

var backupService = app.Services.GetRequiredService<SqliteBackupService>();
await backupService.RestoreLatestBackup();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    context.Database.EnsureCreated();
}
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    // Running in Cloud Run
    app.Urls.Add($"http://0.0.0.0:{port}");
}
app.Run();

