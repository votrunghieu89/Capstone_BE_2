using Capstone_2_BE.DTOs.Invoices;
using Capstone_2_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_2_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;

        public InvoiceController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("banks")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> GetBanks()
        {
            var result = await _invoiceService.GetBanksAsync();
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("check-invoice/{orderId}")]
        public async Task<IActionResult> CheckIsInvoice(Guid orderId)
        {
            var result = await _invoiceService.checkIsInvoice(orderId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("technician/{technicianId}/completed-orders")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> GetAllCompletedOrders(Guid technicianId)
        {
            var result = await _invoiceService.getAllCompletedOrder(technicianId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("customer/{customerId}/completed-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAllCompletedOrdersForCustomer(Guid customerId)
        {
            var result = await _invoiceService.getAllCompletedOrderforCustomer(customerId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid input data" });
            var result = await _invoiceService.CreateInvoice(dto);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { message = "Hóa đơn được tạo thành công." });
        }

        [HttpDelete("delete/{invoiceId}")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> DeleteInvoice(Guid invoiceId)
        {
            var result = await _invoiceService.DeleteInvoice(invoiceId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { message = "Xóa hóa đơn thành công." });
        }

        [HttpGet("detail/{orderId}")]
        public async Task<IActionResult> GetInvoiceDetail(Guid orderId)
        {
            var result = await _invoiceService.GetDetailInvoicebyOrderId(orderId);
             if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
             return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("is-payment/{invoiceId}")]
        public async Task<IActionResult> IsPayment(Guid invoiceId)
        {
            var result = await _invoiceService.IsPayment(invoiceId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPut("confirm-payment/{invoiceId}")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> ConfirmPayment(Guid invoiceId)
        {
            var result = await _invoiceService.ConfirmPayment(invoiceId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { message = "Xác nhận thanh toán thành công." });
        }

        [HttpGet("admin/all-invoices")]
        public async Task<IActionResult> GetAllInvoice()
        {
            var result = await _invoiceService.GetAllInvoice();
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("update-info/{orderId}")]

        public async Task<IActionResult> GetInvoiceItemForUpdate(Guid orderId)
        {
            var result = await _invoiceService.GetInvoiceItemforUpdate(orderId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPut("update/{orderId}")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> UpdateInvoice(Guid orderId, [FromBody] CreateInvoiceDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid input data" });
            dto.OrderId = orderId; 
            var result = await _invoiceService.UpdateInvoice(orderId, dto);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { message = "Cập nhật hóa đơn thành công." });
        }
    }
}
