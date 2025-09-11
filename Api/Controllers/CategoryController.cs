using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var userId = GetCurrentUserId();
            var categories = await _categoryService.GetCategoriesAsync(userId);
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategory request)
        {
            try 
            {
                var userId = GetCurrentUserId();
                var category = await _categoryService.CreateCategoryAsync(request, userId);

                TriggerBackup();

                return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
            } 
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategory request)
        {
            try 
            {
                var userId = GetCurrentUserId();
                var category = await _categoryService.UpdateCategoryAsync(id, request, userId);

                TriggerBackup();

                return Ok(category);
            } 
            catch (ArgumentException ex) 
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _categoryService.DeleteCategoryAsync(id, userId);

                TriggerBackup();

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("categorize")]
        public async Task<IActionResult> CategorizeTransactions([FromBody] Categorize request)
        {
            try 
            {
                var userId = GetCurrentUserId();
                var result = await _categoryService.CategorizeTransactionsAsync(request, userId);
                return Ok(result);
            } 
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("categorize-with-pattern")]
        public async Task<IActionResult> CategorizeWithPattern([FromBody] CategorizeWithPattern request)
        {
            try 
            {
                var userId = GetCurrentUserId();
                var result = await _categoryService.CategorizeWithPatternAsync(request, userId);

                TriggerBackup();

                return Ok(result);
            } 
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
