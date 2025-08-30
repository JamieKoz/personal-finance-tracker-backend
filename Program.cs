using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Persistence;
using PersonalFinanceTracker.Application;
using PersonalFinanceTracker.Api;
using PersonalFinanceTracker.Infrastructure;
using PersonalFinanceTracker.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

var app = builder.Build();

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
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

var t = new Transaction();
