using ProductsService.DTOs;

namespace ProductsService.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string createdBy);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string updatedBy);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
        Task<bool> UpdateStockAsync(int id, int quantity);
    }
}