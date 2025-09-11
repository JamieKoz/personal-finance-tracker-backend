using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PersonalFinanceTracker.Persistence;

namespace PersonalFinanceTracker.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User not found");
        }

        protected string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        protected void TriggerBackup()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var backupService = HttpContext.RequestServices.GetRequiredService<SqliteBackupService>();
                    await backupService.BackupDatabase();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Backup failed: {ex.Message}");
                }
            });
        }
    }
}
