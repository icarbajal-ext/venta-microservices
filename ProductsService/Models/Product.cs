using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductsService.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        [StringLength(50)]
        public string? SKU { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }

    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}