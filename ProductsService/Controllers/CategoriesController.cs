using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsService.DTOs;
using ProductsService.Services;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Obtener todas las categorías (endpoint público)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Obtener categoría por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            
            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(category);
        }

        /// <summary>
        /// Crear nueva categoría (requiere autenticación)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar categoría (requiere autenticación)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            
            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(category);
        }

        /// <summary>
        /// Eliminar categoría (solo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                
                if (!success)
                    return NotFound(new { message = "Categoría no encontrada" });

                return Ok(new { message = "Categoría eliminada exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}