using System.Text.Json;
using Capstone_2_BE.DTOs.Invoices;
using Capstone_2_BE.Repositories;

namespace Capstone_2_BE.Services
{
    public class InvoiceService
    {
        private readonly IInvoiceRepo _invoiceRepo;
        private readonly HttpClient _httpClient;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IInvoiceRepo invoiceRepo, HttpClient httpClient, ILogger<InvoiceService> logger)
        {
            _invoiceRepo = invoiceRepo;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<bool>> checkIsInvoice(Guid OrderId)
        {
            try
            {
                var result = await _invoiceRepo.checkIsInvoice(OrderId);
                return Result<bool>.Success(result, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking invoice for OrderId {OrderId}", OrderId);
                return Result<bool>.Failure("Lỗi khi kiểm tra trạng thái hóa đơn", 500);
            }
        }

        public async Task<Result<List<ViewAllCompletedOrderDTO>>> getAllCompletedOrder(Guid TechnicianId)
        {
            try
            {
                var result = await _invoiceRepo.getAllCompletedOrder(TechnicianId);
                return Result<List<ViewAllCompletedOrderDTO>>.Success(result, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed orders for Technician {TechnicianId}", TechnicianId);
                return Result<List<ViewAllCompletedOrderDTO>>.Failure("Lỗi khi lấy danh sách đơn hoàn thành", 500);
            }
        }

        public async Task<Result<bool>> CreateInvoice(CreateInvoiceDTO create)
        {
            try
            {
                var result = await _invoiceRepo.CreateInvoice(create);
                if (result)
                {
                    return Result<bool>.Success(true, 200);
                }
                return Result<bool>.Failure("Không thể tạo hóa đơn hoặc hóa đơn đã tồn tại", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for OrderId {OrderId}", create.OrderId);
                return Result<bool>.Failure("Lỗi hệ thống khi tạo hóa đơn", 500);
            }
        }

        public async Task<Result<bool>> DeleteInvoice(Guid InvoiceId)
        {
            try
            {
                var result = await _invoiceRepo.DeleteInvoice(InvoiceId);
                if (result)
                {
                    return Result<bool>.Success(true, 200);
                }
                return Result<bool>.Failure("Không tìm thấy hóa đơn cần xóa", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}", InvoiceId);
                return Result<bool>.Failure("Lỗi hệ thống khi xóa hóa đơn", 500);
            }
        }

        public async Task<Result<ViewDetailInvoiceDTO>> GetDetailInvoicebyOrderId(Guid OrderId)
        {
            try
            {
                var result = await _invoiceRepo.GetDetailInvoicebyOrderId(OrderId);
                if (result != null)
                {
                    return Result<ViewDetailInvoiceDTO>.Success(result, 200);
                }
                return Result<ViewDetailInvoiceDTO>.Failure("Không tìm thấy thông tin hóa đơn", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice details for OrderId {OrderId}", OrderId);
                return Result<ViewDetailInvoiceDTO>.Failure("Lỗi hệ thống khi lấy chi tiết hóa đơn", 500);
            }
        }

        public async Task<Result<List<BankIDDTO>>> GetBanksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.vietqr.io/v2/banks");

                if (!response.IsSuccessStatusCode)
                {
                    return Result<List<BankIDDTO>>.Failure("Không thể lấy danh sách ngân hàng từ VietQR", (int)response.StatusCode);
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;

                var banksList = new List<BankIDDTO>();

                if (root.TryGetProperty("data", out JsonElement dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in dataElement.EnumerateArray())
                    {
                        var bankDTO = new BankIDDTO
                        {
                            BankCode = element.GetProperty("code").GetString() ?? string.Empty,
                            BankName = element.GetProperty("name").GetString() ?? string.Empty
                        };
                        banksList.Add(bankDTO);
                    }
                }

                return Result<List<BankIDDTO>>.Success(banksList, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve banks from VietQR API.");
                return Result<List<BankIDDTO>>.Failure("Lỗi hệ thống khi tải danh sách ngân hàng", 500);
            }
        }

        public async Task<Result<bool>> IsPayment(Guid InvoiceId)
        {
            try
            {
                var result = await _invoiceRepo.IsPayment(InvoiceId);
                return Result<bool>.Success(result, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for InvoiceId {InvoiceId}", InvoiceId);
                return Result<bool>.Failure("Lỗi hệ thống khi kiểm tra trạng thái thanh toán", 500);
            }
        }

        public async Task<Result<bool>> ConfirmPayment(Guid InvoiceId)
        {
            try
            {
                var result = await _invoiceRepo.ConfirmPayment(InvoiceId);
                if (result)
                {
                    return Result<bool>.Success(true, 200);
                }
                return Result<bool>.Failure("Không tìm thấy hóa đơn cần cập nhật", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for InvoiceId {InvoiceId}", InvoiceId);
                return Result<bool>.Failure("Lỗi hệ thống khi cập nhật trạng thái thanh toán", 500);
            }
        }

        public async Task<Result<List<ViewAllInvoice>>> GetAllInvoice()
        {
            try
            {
                var result = await _invoiceRepo.GetAllInvoice();
                return Result<List<ViewAllInvoice>>.Success(result, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices for admin");
                return Result<List<ViewAllInvoice>>.Failure("Lỗi hệ thống khi lấy danh sách hóa đơn", 500);
            }
        }

        public async Task<Result<ViewUpdateInvoiceDTO>> GetInvoiceItemforUpdate(Guid OrderId)
        {
            try
            {
                var result = await _invoiceRepo.GetInvoiceItemforUpdate(OrderId);
                if (result != null)
                {
                    return Result<ViewUpdateInvoiceDTO>.Success(result, 200);
                }
                return Result<ViewUpdateInvoiceDTO>.Failure("Không tìm thấy thông tin hóa đơn để cập nhật", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice update info for OrderId {OrderId}", OrderId);
                return Result<ViewUpdateInvoiceDTO>.Failure("Lỗi hệ thống khi lấy thông tin cập nhật hóa đơn", 500);
            }
        }

        public async Task<Result<bool>> UpdateInvoice(Guid OrderId, CreateInvoiceDTO createInvoiceDTO)
        {
            try
            {
                var result = await _invoiceRepo.UpdateInvoice(OrderId, createInvoiceDTO);
                if (result)
                {
                    return Result<bool>.Success(true, 200);
                }
                return Result<bool>.Failure("Không tìm thấy đơn hàng cần cập nhật", 404);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice for OrderId {OrderId}", OrderId);
                return Result<bool>.Failure("Lỗi hệ thống khi cập nhật hóa đơn", 500);
            }
        }
    }
}
