using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
using PaymentsService.DTOs;
using PaymentsService.Models;

namespace PaymentsService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentsDbContext _context;

        public PaymentService(PaymentsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.PaymentMethod)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToPaymentDto);
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            return payment == null ? null : MapToPaymentDto(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(string userId)
        {
            var payments = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToPaymentDto);
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto, string userId)
        {
            var payment = new Payment
            {
                OrderId = createPaymentDto.OrderId,
                Amount = createPaymentDto.Amount,
                PaymentMethodId = createPaymentDto.PaymentMethodId,
                Description = createPaymentDto.Description,
                Reference = createPaymentDto.Reference,
                UserId = userId,
                Status = "Pending",
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await _context.Entry(payment)
                .Reference(p => p.PaymentMethod)
                .LoadAsync();

            return MapToPaymentDto(payment);
        }

        public async Task<PaymentDto?> UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto updateStatusDto)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return null;

            payment.Status = updateStatusDto.Status;
            payment.TransactionId = updateStatusDto.TransactionId;
            payment.UpdatedAt = DateTime.UtcNow;

            if (updateStatusDto.Status == "Completed")
            {
                payment.ProcessedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToPaymentDto(payment);
        }

        public async Task<IEnumerable<PaymentDto>> SearchPaymentsAsync(PaymentSearchDto searchDto)
        {
            var query = _context.Payments
                .Include(p => p.PaymentMethod)
                .AsQueryable();

            if (searchDto.OrderId.HasValue)
                query = query.Where(p => p.OrderId == searchDto.OrderId);

            if (!string.IsNullOrEmpty(searchDto.Status))
                query = query.Where(p => p.Status == searchDto.Status);

            if (searchDto.PaymentMethodId.HasValue)
                query = query.Where(p => p.PaymentMethodId == searchDto.PaymentMethodId);

            if (searchDto.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= searchDto.MinAmount);

            if (searchDto.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= searchDto.MaxAmount);

            if (searchDto.FromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= searchDto.FromDate);

            if (searchDto.ToDate.HasValue)
                query = query.Where(p => p.PaymentDate <= searchDto.ToDate);

            if (!string.IsNullOrEmpty(searchDto.UserId))
                query = query.Where(p => p.UserId == searchDto.UserId);

            query = query.Skip((searchDto.Page - 1) * searchDto.PageSize)
                         .Take(searchDto.PageSize);

            var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return payments.Select(MapToPaymentDto);
        }

        public async Task<PaymentDto?> ProcessPaymentAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null || payment.Status != "Pending") return null;

            // Simulate payment processing
            payment.Status = "Completed";
            payment.ProcessedAt = DateTime.UtcNow;
            payment.TransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{payment.Id}";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToPaymentDto(payment);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToPaymentDto);
        }

        private static PaymentDto MapToPaymentDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethod.Name,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                Description = payment.Description,
                PaymentDate = payment.PaymentDate,
                ProcessedAt = payment.ProcessedAt,
                UserId = payment.UserId,
                Reference = payment.Reference,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}