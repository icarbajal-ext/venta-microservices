using System.ComponentModel.DataAnnotations;

namespace ProductsService.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CreateProductDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; } = 0;

        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }

    public class UpdateProductDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ProductSearchDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}