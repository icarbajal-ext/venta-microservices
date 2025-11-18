using PaymentsService.DTOs;

namespace PaymentsService.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(string userId);
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto, string userId);
        Task<PaymentDto?> UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto updateStatusDto);
        Task<IEnumerable<PaymentDto>> SearchPaymentsAsync(PaymentSearchDto searchDto);
        Task<PaymentDto?> ProcessPaymentAsync(int id);
        Task<decimal> GetTotalRevenueAsync();
        Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId);
    }
}