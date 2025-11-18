using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.DTOs;
using ProductsService.Models;

namespace ProductsService.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductsDbContext _context;

        public ProductService(ProductsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return products.Select(MapToProductDto);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return product == null ? null : MapToProductDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string createdBy)
        {
            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                CategoryId = createProductDto.CategoryId,
                SKU = createProductDto.SKU,
                Stock = createProductDto.Stock,
                ImageUrl = createProductDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Reload with category
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return MapToProductDto(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string updatedBy)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return null;

            // Update only non-null properties
            if (!string.IsNullOrEmpty(updateProductDto.Name))
                product.Name = updateProductDto.Name;

            if (updateProductDto.Description != null)
                product.Description = updateProductDto.Description;

            if (updateProductDto.Price.HasValue)
                product.Price = updateProductDto.Price.Value;

            if (updateProductDto.CategoryId.HasValue)
                product.CategoryId = updateProductDto.CategoryId.Value;

            if (!string.IsNullOrEmpty(updateProductDto.SKU))
                product.SKU = updateProductDto.SKU;

            if (updateProductDto.Stock.HasValue)
                product.Stock = updateProductDto.Stock.Value;

            if (updateProductDto.ImageUrl != null)
                product.ImageUrl = updateProductDto.ImageUrl;

            if (updateProductDto.IsActive.HasValue)
                product.IsActive = updateProductDto.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            // Reload category if changed
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return MapToProductDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive) return false;

            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.Search))
            {
                query = query.Where(p => p.Name.Contains(searchDto.Search) || 
                                        p.Description!.Contains(searchDto.Search));
            }

            if (searchDto.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == searchDto.CategoryId);
            }

            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= searchDto.MinPrice);
            }

            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= searchDto.MaxPrice);
            }

            if (searchDto.InStock.HasValue && searchDto.InStock.Value)
            {
                query = query.Where(p => p.Stock > 0);
            }

            // Apply pagination
            query = query.Skip((searchDto.Page - 1) * searchDto.PageSize)
                         .Take(searchDto.PageSize);

            var products = await query.OrderBy(p => p.Name).ToListAsync();
            return products.Select(MapToProductDto);
        }

        public async Task<bool> UpdateStockAsync(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive) return false;

            product.Stock = quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SKU = product.SKU,
                Stock = product.Stock,
                IsActive = product.IsActive,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy
            };
        }
    }
}