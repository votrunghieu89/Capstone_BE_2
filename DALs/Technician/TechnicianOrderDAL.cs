using Capstone_2_BE.DTOs.Technician.Orders;
using Capstone_2_BE.Models;
using Capstone_2_BE.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Capstone_2_BE.DALs.Technician
{
    public class TechnicianOrderDAL : ITechnicianOrderRepo
    {
        public readonly AppDbContext _context;
        public readonly ILogger<TechnicianOrderDAL> _logger;

        public TechnicianOrderDAL(AppDbContext context, ILogger<TechnicianOrderDAL> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<bool> CompleteOrder(Guid orderId, Guid AccountId)
        {
            try
            {
                
               using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int isUpdated = await _context.OrderrModel.Where(o => o.Id == orderId && o.Status == "In Progress").ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Completed"));
                        if (isUpdated > 0)
                        {
                            OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                            {
                                OrderId = orderId,
                                Status = "Completed",
                                ChangeBy = AccountId,
                                ChangeAt = DateTime.UtcNow,
                            };
                            await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return true;
                        }
                        return false;
                    }
                    catch(Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> ConfirmOrder(Guid orderId, Guid AccountId)
        {
            try
            {

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int isUpdated = await _context.OrderrModel.Where(o => o.Id == orderId && o.Status == "Pending Confirmation").ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Confirmed"));
                        if (isUpdated > 0)
                        {
                            OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                            {
                                OrderId = orderId,
                                Status = "Confirmed",
                                ChangeBy = AccountId,
                                ChangeAt = DateTime.UtcNow,
                            };
                            await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> StartOrder(Guid orderId, Guid AccountId)
        {
            try
            {

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int isUpdated = await _context.OrderrModel.Where(o => o.Id == orderId && o.Status == "Confirmed").ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "In Progress"));
                        if (isUpdated > 0)
                        {
                            OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                            {
                                OrderId = orderId,
                                Status = "In Progress",
                                ChangeBy = AccountId,
                                ChangeAt = DateTime.UtcNow,
                            };
                            await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        // Lấy ds đơn đã xác nhận
        public async Task<List<ViewOrderDTO>> GetConfirmedOrders(Guid technicianId)
        {
            try
            {
                List<ViewOrderDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                           join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                           join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                                           where o.TechnicianId == technicianId && o.Status == "Confirmed"
                                                           select new ViewOrderDTO
                                                           {
                                                               OrderId = o.Id,
                                                               CustomerName = c.FullName,
                                                               ServiceName = s.ServiceName,
                                                               Title = o.Title,
                                                               Status = o.Status,
                                                               OrderDate = o.CreateAt,
                                                           }).ToListAsync();
                return InProgressOrder;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // Lấy ds đơn đang chờ xác nhận
        public async Task<List<ViewOrderDTO>> GetConfirmingOrders(Guid technicianId)
        {
            try
            {
                List<ViewOrderDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                           join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                           join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                                           where o.TechnicianId == technicianId && o.Status == "Pending Confirmation"
                                                           select new ViewOrderDTO
                                                           {
                                                               OrderId = o.Id,
                                                               CustomerName = c.FullName,
                                                               ServiceName = s.ServiceName,
                                                               Title = o.Title,
                                                               Status = o.Status,
                                                               OrderDate = o.CreateAt,
                                                           }).ToListAsync();
                return InProgressOrder;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // Lấy toàn bộ đơn đã hoàn thành 
        public async Task<List<ViewOrderDTO>> GetHistoryOrders(Guid technicianId)
        {
            try
            {
                List<ViewOrderDTO> InProgressOrder = await (from o in _context.OrderrModel
                                            join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                            join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                            where o.TechnicianId == technicianId && o.Status == "Completed"
                                            select new ViewOrderDTO
                                            {
                                                OrderId = o.Id,
                                                CustomerName = c.FullName,
                                                ServiceName = s.ServiceName,
                                                Title = o.Title,
                                                Status = o.Status,
                                                OrderDate = o.CreateAt,
                                            }).ToListAsync();
                return InProgressOrder;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // Lây toàn bộ đơn đang thực hiện
        public async Task<ViewOrderDTO> GetInProgressOrders(Guid technicianId)
        {
            try
            {
                var InProgressOrder = await (from o in _context.OrderrModel
                                        join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                        join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                        where o.TechnicianId == technicianId && o.Status == "In Progress"
                                        select new ViewOrderDTO
                                        {
                                            OrderId = o.Id,
                                            CustomerName = c.FullName,
                                            ServiceName = s.ServiceName,
                                            Title = o.Title,
                                            Status = o.Status,
                                            OrderDate = o.CreateAt,
                                        }).FirstOrDefaultAsync();
                return InProgressOrder;
            }
            catch (Exception ex)
            {
               return null;
            }   
        }

        public async Task<List<ViewOrderDTO>> GetCanceledOrders(Guid technicianId)
        {
            try
            {
                List<ViewOrderDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                           join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                           join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                                           where o.TechnicianId == technicianId && o.Status == "Refuse"
                                                           select new ViewOrderDTO
                                                           {
                                                               OrderId = o.Id,
                                                               CustomerName = c.FullName,
                                                               ServiceName = s.ServiceName,
                                                               Title = o.Title,
                                                               Status = o.Status,
                                                               OrderDate = o.CreateAt,
                                                           }).ToListAsync();
                return InProgressOrder;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> CancelOrder(Guid orderId, Guid AccountId)
        {
            try
            {

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int isUpdated = await _context.OrderrModel.Where(o => o.Id == orderId && o.Status == "Pending Confirmation").ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Refuse"));
                        if (isUpdated > 0)
                        {
                            OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                            {
                                OrderId = orderId,
                                Status = "Refuse",
                                ChangeBy = AccountId,
                                ChangeAt = DateTime.UtcNow,
                            };
                            await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
