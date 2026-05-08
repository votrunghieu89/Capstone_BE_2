using Capstone_2_BE.DTOs.Invoices;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace Capstone_2_BE.DALs
{
    public class InvoiceDAL : IInvoiceRepo
    {
        public readonly AppDbContext _context;
        public readonly ILogger<InvoiceDAL> _logger;

        public InvoiceDAL(AppDbContext context, ILogger<InvoiceDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> checkIsInvoice(Guid OrderId) // kiểm tra xem isInvoice là 1 hay 0: 1 là đã tạo hóa đơn, 0 là chưa tạo hóa đơn
        {
            var order = await _context.OrderrModel.FirstOrDefaultAsync(o => o.Id == OrderId);
            if (order == null) return false;
            return order.IsInvoice == 1;
        }

        public async Task<bool> IsPayment(Guid InvoiceId) // Chỉ kiểm tra xem đơn hàng là 0 hay 1, 1 trả về true, 0 trả về false, 1 là đã thanh toán, 0 là chưa thanh toán
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == InvoiceId);
            if (invoice == null) return false;

            if(invoice.PaymentStatus == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ConfirmPayment(Guid InvoiceId) // Cập nhật lại PaymentStatus thành 1 (đã thanh toán)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == InvoiceId);
            if (invoice == null) return false;

            invoice.PaymentStatus = 1;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CreateInvoice(CreateInvoiceDTO create) // tạo hóa đơn, sau khi tạo hóa đơn xong thì cập nhật lại isInvoice của order thành 1, nếu tạo hóa đơn thất bại thì isInvoice vẫn là 0, dùng TransactionScope để đảm bảo tính toàn vẹn dữ liệu
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.OrderrModel.FirstOrDefaultAsync(o => o.Id == create.OrderId);
                if (order == null || order.IsInvoice == 1)
                {
                    return false; // Order not found or already has an invoice
                }

                decimal totalMaterials = create.Materials.Sum(m => m.Price * m.Quantity);
                decimal totalAmount = totalMaterials + create.LaborCost;

                InvoicesModel invoice = new InvoicesModel
                {
                    OrderId = create.OrderId,
                    LaborCost = create.LaborCost,
                    TotalAmount = totalAmount,
                    BankCode = create.BankCode,
                    BankAccount = create.BankAccount,
                    BankAccountName = create.BankAccountName,
                    PaymentStatus = 0, // Mặc định là chưa thanh toán
                    CreatedAt = DateTime.Now
                };

                await _context.Invoices.AddAsync(invoice);

                if (create.Materials.Any())
                {
                    var invoiceItems = create.Materials.Select(m => new InvoiceItemsModel
                    {
                        InvoiceId = invoice.Id,
                        MaterialName = m.MaterialName,
                        Price = m.Price,
                        Quantity = m.Quantity,
                        Subtotal = m.Price * m.Quantity,
                        CreatedAt = DateTime.Now
                    }).ToList();

                    await _context.InvoiceItems.AddRangeAsync(invoiceItems);
                }

                order.IsInvoice = 1;
                _context.OrderrModel.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for OrderId: {OrderId}", create.OrderId);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteInvoice(Guid InvoiceId) // xóa hóa đơn, sau khi xóa hóa đơn xong thì cập nhật lại isInvoice của order thành 0, dùng TransactionScope để đảm bảo tính toàn vẹn dữ liệu
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == InvoiceId);
                if (invoice == null) return false;

                var order = await _context.OrderrModel.FirstOrDefaultAsync(o => o.Id == invoice.OrderId);
                if (order != null)
                {
                   var isUpdate = await _context.OrderrModel.Where(o => o.Id == order.Id).ExecuteUpdateAsync(e => e.SetProperty(sg => sg.IsInvoice, 0));
                }

                var isDeletedItems = await _context.InvoiceItems.Where(i => i.InvoiceId == InvoiceId).ExecuteDeleteAsync();
                var isDeletedInvoice = await _context.Invoices.Where(i => i.Id == InvoiceId).ExecuteDeleteAsync();
                if (isDeletedItems == 0 || isDeletedInvoice == 0) // Nếu có lỗi khi xóa các mục hóa đơn, rollback và trả về false
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice: {InvoiceId}", InvoiceId);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<ViewAllCompletedOrderDTO>> getAllCompletedOrder(Guid TechnicianId) // Hiển thị tất cả các order đã hoàn thành của 1 kĩ thuật viên
        {
            return await _context.OrderrModel
                .Where(o => o.TechnicianId == TechnicianId && o.Status == "Completed")
                .Select(o => new ViewAllCompletedOrderDTO
                {
                    OrderId = o.Id,
                    CustomerId = o.CustomerId,
                    TechnicianID = o.TechnicianId,
                    Title = o.Title,
                    Status = o.Status,
                    CreatedAt = o.CreateAt
                })
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<ViewDetailInvoiceDTO> GetDetailInvoicebyOrderId(Guid OrderId) // Hiển thị chi tiết hóa đơn của 1 order
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Order)
                    .ThenInclude(o => o.CustomerProfile)
                .Include(i => i.Order)
                    .ThenInclude(o => o.TechnicianProfile)
                .Include(i => i.Order)
                    .ThenInclude(o => o.ServiceCategories)
                .Include(i => i.Order)
                    .ThenInclude(o => o.Cities)
                .FirstOrDefaultAsync(i => i.OrderId == OrderId);

            if (invoice == null) return null;

            var order = invoice.Order;

            string qrCodeUrl = "";
            if (!string.IsNullOrEmpty(invoice.BankCode) && !string.IsNullOrEmpty(invoice.BankAccount))
            {
                string description = Uri.EscapeDataString($"Thanh toan hoa don sua chua: {order.Title}");
                string accountName = Uri.EscapeDataString(invoice.BankAccountName ?? "");
                qrCodeUrl = $"https://img.vietqr.io/image/{invoice.BankCode}-{invoice.BankAccount}-compact2.png?amount={Math.Round(invoice.TotalAmount)}&addInfo={description}&accountName={accountName}";
            }

            return new ViewDetailInvoiceDTO
            {
                InvoiceId = invoice.Id,
                NameCustomer = order.CustomerProfile?.FullName ?? "",
                NameTechnician = order.TechnicianProfile?.FullName ?? "",
                ServiceName = order.ServiceCategories?.ServiceName ?? "",
                AdressOrder = order.Address,
                CityNameOrder = order.Cities?.CityName ?? "",
                CustomerPhone = order.CustomerProfile?.PhoneNumber ?? "",
                LaborCost = invoice.LaborCost,
                TotalAmount = invoice.TotalAmount,
                QRCode = qrCodeUrl,
                PaymentStatus = invoice.PaymentStatus,
                CreatedAt = invoice.CreatedAt,
                Materials = invoice.InvoiceItems?.Select(m => new ViewMaterialItemDTO
                {
                    MaterialName = m.MaterialName,
                    Price = m.Price,
                    Quantity = m.Quantity,
                    Subtotal = m.Subtotal
                }).ToList() ?? new List<ViewMaterialItemDTO>()
            };
        }

        public async Task<List<ViewAllInvoice>> GetAllInvoice() // này cho ADmin, hiển thị tất cả hoá đơn
        {
            return await _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.CustomerProfile)
                .Include(i => i.Order)
                    .ThenInclude(o => o.TechnicianProfile)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new ViewAllInvoice
                {
                    InvoiceId = i.Id,
                    OrderId = i.OrderId,
                    CustomerName = i.Order.CustomerProfile != null ? i.Order.CustomerProfile.FullName : "",
                    TechnicianName = i.Order.TechnicianProfile != null ? i.Order.TechnicianProfile.FullName : "",
                    PaymentStatus = i.PaymentStatus,
                    TotalAmount = i.TotalAmount,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ViewUpdateInvoiceDTO> GetInvoiceItemforUpdate(Guid OrderId) // này là lấy detail của hóa đơn để hiển thị lên form update
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.OrderId == OrderId);

            if (invoice == null) return null;

            return new ViewUpdateInvoiceDTO
            {
                OrderId = invoice.OrderId,
                InvoiceId = invoice.Id,
                LaborCost = invoice.LaborCost,
                TotalAmount = invoice.TotalAmount,
                BankCode = invoice.BankCode ?? "",
                BankAccount = invoice.BankAccount ?? "",
                BankAccountName = invoice.BankAccountName ?? "",
                CreatedAt = invoice.CreatedAt,
                Materials = invoice.InvoiceItems?.Select(m => new ViewUpdateInvoiceDTO.ViewMaterialItemDTO
                {
                    MaterialName = m.MaterialName,
                    Price = m.Price,
                    Quantity = m.Quantity,
                    Subtotal = m.Subtotal
                }).ToList() ?? new List<ViewUpdateInvoiceDTO.ViewMaterialItemDTO>()
            };
        }

        public async Task<bool> UpdateInvoice(Guid OrderId, CreateInvoiceDTO createInvoiceDTO) // khi update tuân theo quy tắc sau. Khi update thì sẽ xóa hết các item cũ đi và thêm các item mới vào, Chỉ được update khi hoá đơn chưa thanh toán, Bảng Invoices chỉ update lại thông tin, ko xoá nó, ko update lại CreatedAt, chỉ update LaborCost, TotalAmount, BankCode, BankAccount, BankAccountName, PaymentStatus, còn bảng InvoiceItems thì sẽ xóa hết các item cũ đi và thêm các item mới vào
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingInvoice = await _context.Invoices
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.OrderId == OrderId);

                // Chỉ cho phép cập nhật khi hóa đơn chưa thanh toán (PaymentStatus == 0)
                if (existingInvoice == null || existingInvoice.PaymentStatus == 1)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Xóa tất cả InvoiceItems cũ
                if (existingInvoice.InvoiceItems != null && existingInvoice.InvoiceItems.Any())
                {
                    _context.InvoiceItems.RemoveRange(existingInvoice.InvoiceItems);
                }

                // Cập nhật lại các thông tin của Invoices
                decimal totalMaterials = createInvoiceDTO.Materials.Sum(m => m.Price * m.Quantity);
                decimal totalAmount = totalMaterials + createInvoiceDTO.LaborCost;

                existingInvoice.LaborCost = createInvoiceDTO.LaborCost;
                existingInvoice.TotalAmount = totalAmount;
                existingInvoice.BankCode = createInvoiceDTO.BankCode;
                existingInvoice.BankAccount = createInvoiceDTO.BankAccount;
                existingInvoice.BankAccountName = createInvoiceDTO.BankAccountName;
                
                _context.Invoices.Update(existingInvoice);

                // Thêm InvoiceItem mới vào
                if (createInvoiceDTO.Materials != null && createInvoiceDTO.Materials.Any())
                {
                    var newInvoiceItems = createInvoiceDTO.Materials.Select(m => new InvoiceItemsModel
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = existingInvoice.Id,
                        MaterialName = m.MaterialName,
                        Price = m.Price,
                        Quantity = m.Quantity,
                        Subtotal = m.Price * m.Quantity,
                        CreatedAt = DateTime.Now
                    }).ToList();

                    await _context.InvoiceItems.AddRangeAsync(newInvoiceItems);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice for OrderId: {OrderId}", OrderId);
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
