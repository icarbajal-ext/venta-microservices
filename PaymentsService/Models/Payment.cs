using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentsService.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [ForeignKey("PaymentMethodId")]
        public PaymentMethod PaymentMethod { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Reference { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    [Table("PaymentMethods")]
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProcessingFee { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}