using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Persistence;

namespace PersonalFinanceTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        [HttpPost("backup")]
        public async Task<IActionResult> TriggerBackup()
        {
            try
            {
                var backupService = HttpContext.RequestServices.GetRequiredService<SqliteBackupService>();
                await backupService.BackupDatabase();
                return Ok(new { message = "Backup completed successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
