using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.DTOs;
using ProductsService.Models;

namespace ProductsService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ProductsDbContext _context;

        public CategoryService(ProductsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToCategoryDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            return category == null ? null : MapToCategoryDto(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToCategoryDto(category);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null) return null;

            if (!string.IsNullOrEmpty(updateCategoryDto.Name))
                category.Name = updateCategoryDto.Name;

            if (updateCategoryDto.Description != null)
                category.Description = updateCategoryDto.Description;

            if (updateCategoryDto.IsActive.HasValue)
                category.IsActive = updateCategoryDto.IsActive.Value;

            await _context.SaveChangesAsync();
            return MapToCategoryDto(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null || !category.IsActive) return false;

            // Check if category has products
            if (category.Products.Any(p => p.IsActive))
            {
                throw new InvalidOperationException("Cannot delete category that contains active products");
            }

            // Soft delete
            category.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                ProductCount = category.Products?.Count(p => p.IsActive) ?? 0
            };
        }
    }
}