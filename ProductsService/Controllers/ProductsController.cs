using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProductsService.DTOs;
using ProductsService.Services;

namespace ProductsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Obtener todos los productos (endpoint público)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Buscar productos con filtros
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] ProductSearchDto searchDto)
        {
            var products = await _productService.SearchProductsAsync(searchDto);
            return Ok(products);
        }

        /// <summary>
        /// Obtener producto por ID (requiere autenticación)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }

        /// <summary>
        /// Crear nuevo producto (requiere autenticación)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBy = GetCurrentUsername();
            
            try
            {
                var product = await _productService.CreateProductAsync(createProductDto, createdBy);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar producto (requiere autenticación)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedBy = GetCurrentUsername();
            var product = await _productService.UpdateProductAsync(id, updateProductDto, updatedBy);
            
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }

        /// <summary>
        /// Eliminar producto (solo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            
            if (!success)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(new { message = "Producto eliminado exitosamente" });
        }

        /// <summary>
        /// Actualizar stock del producto (requiere autenticación)
        /// </summary>
        [HttpPatch("{id}/stock")]
        [Authorize]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto updateStockDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _productService.UpdateStockAsync(id, updateStockDto.Quantity);
            
            if (!success)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(new { message = "Stock actualizado exitosamente" });
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        }
    }

    public class UpdateStockDto
    {
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}