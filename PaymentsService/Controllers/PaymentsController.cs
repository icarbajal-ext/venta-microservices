using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PaymentsService.DTOs;
using PaymentsService.Services;

namespace PaymentsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Obtener todos los pagos (Solo Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        /// <summary>
        /// Buscar pagos con filtros
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> SearchPayments([FromQuery] PaymentSearchDto searchDto)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Los usuarios normales solo pueden buscar sus propios pagos
            if (currentUserRole != "Admin" && searchDto.UserId != currentUserId)
            {
                searchDto.UserId = currentUserId;
            }

            var payments = await _paymentService.SearchPaymentsAsync(searchDto);
            return Ok(payments);
        }

        /// <summary>
        /// Obtener pago por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            
            if (payment == null)
                return NotFound(new { message = "Pago no encontrado" });

            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Los usuarios solo pueden ver sus propios pagos, excepto los admins
            if (currentUserRole != "Admin" && payment.UserId != currentUserId)
            {
                return Forbid();
            }

            return Ok(payment);
        }

        /// <summary>
        /// Obtener pagos del usuario actual
        /// </summary>
        [HttpGet("my-payments")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetMyPayments()
        {
            var userId = GetCurrentUserId();
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments);
        }

        /// <summary>
        /// Obtener pagos por Order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByOrder(int orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Filtrar pagos del usuario actual si no es admin
            if (currentUserRole != "Admin")
            {
                payments = payments.Where(p => p.UserId == currentUserId);
            }

            return Ok(payments);
        }

        /// <summary>
        /// Crear nuevo pago
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto, userId);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar estado del pago (Solo Admin)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.UpdatePaymentStatusAsync(id, updateStatusDto);
            
            if (payment == null)
                return NotFound(new { message = "Pago no encontrado" });

            return Ok(payment);
        }

        /// <summary>
        /// Procesar pago (cambiar a Completed)
        /// </summary>
        [HttpPost("{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentDto>> ProcessPayment(int id)
        {
            var payment = await _paymentService.ProcessPaymentAsync(id);
            
            if (payment == null)
                return NotFound(new { message = "Pago no encontrado o ya procesado" });

            return Ok(payment);
        }

        /// <summary>
        /// Obtener revenue total (Solo Admin)
        /// </summary>
        [HttpGet("revenue/total")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetTotalRevenue()
        {
            var totalRevenue = await _paymentService.GetTotalRevenueAsync();
            return Ok(new { totalRevenue, currency = "USD" });
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}