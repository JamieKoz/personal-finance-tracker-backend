using Microsoft.AspNetCore.Mvc;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.DTO;

namespace PersonalFinanceTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategory request)
        {
            try {
                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
            } catch (ArgumentException ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategory request)
        {
            try {
                var category = await _categoryService.UpdateCategoryAsync(id, request);
                return Ok(category);
            } catch (ArgumentException ex) {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
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
            try {
                var result = await _categoryService.CategorizeTransactionsAsync(request);
                return Ok(result);
            } catch (ArgumentException ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("categorize-with-pattern")]
        public async Task<IActionResult> CategorizeWithPattern([FromBody] CategorizeWithPattern request)
        {
            try {
                var result = await _categoryService.CategorizeWithPatternAsync(request);
                return Ok(result);
            } catch (ArgumentException ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
