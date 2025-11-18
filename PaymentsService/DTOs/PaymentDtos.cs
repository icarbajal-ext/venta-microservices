using System.ComponentModel.DataAnnotations;

namespace PaymentsService.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Description { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }
    }

    public class UpdatePaymentStatusDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(100)]
        public string? TransactionId { get; set; }
    }

    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public decimal? ProcessingFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PaymentCount { get; set; }
    }

    public class CreatePaymentMethodDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal? ProcessingFee { get; set; }
    }

    public class PaymentSearchDto
    {
        public int? OrderId { get; set; }
        public string? Status { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}