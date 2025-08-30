using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.DTO;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Controllers
{
    [Route("api/[controller]")]
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("upload-csv")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try 
            {
                var userId = GetCurrentUserId();
                var result = await _transactionService.ImportCsvAsync(file, userId);
                return Ok(result);
            } 
            catch (Exception ex) 
            {
                return BadRequest($"Error processing CSV: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTransactions(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 50,
         [FromQuery] string? search = null,
         [FromQuery] string? sortField = "date",
         [FromQuery] string? sortDirection = "desc",
         [FromQuery] string? category = null)
        {
            var request = new GetTransactions
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                SortField = sortField,
                SortDirection = sortDirection,
                Category = category
            };

            var userId = GetCurrentUserId();
            var result = await _transactionService.GetTransactionsAsync(request, userId);
            return Ok(result);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetTransactionSummary([FromQuery] bool excludeInternalTransfers = false)
        {
            var userId = GetCurrentUserId();
            var summary = await _transactionService.GetTransactionSummaryAsync(userId, excludeInternalTransfers);
            return Ok(summary);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var userId = GetCurrentUserId();
            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        [HttpPut("{id}/category")]
        public async Task<IActionResult> UpdateTransactionCategory(int id, [FromBody] UpdateTransaction request)
        {
            try 
            {
                var userId = GetCurrentUserId();
                await _transactionService.UpdateTransactionCategoryAsync(id, request.CategoryId, userId);
                return Ok(new { Message = "Transaction category updated successfully" });
            } 
            catch (ArgumentException ex) 
            {
                return NotFound(ex.Message);
            } 
            catch (InvalidOperationException ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
