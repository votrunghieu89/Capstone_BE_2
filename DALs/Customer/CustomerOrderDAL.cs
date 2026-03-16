using Capstone_2_BE.DTOs.Customer.Order;
using Capstone_2_BE.DTOs.Technician.Orders;
using Capstone_2_BE.Models;
using Capstone_2_BE.Repositories.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using StackExchange.Redis;

namespace Capstone_2_BE.DALs.Customer
{
    public class CustomerOrderDAL : ICustomerOrderRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerOrderDAL> _logger;

        public CustomerOrderDAL(AppDbContext context, ILogger<CustomerOrderDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderActionResDTO> CancelOrder(OrderActionDTO orderActionDTO)
        {
            try
            {

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int isUpdated = await _context.OrderrModel.Where(o => o.Id == orderActionDTO.OrderId && o.Status == "Pending Confirmation").ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Confirmed"));
                        if (isUpdated > 0)
                        {
                            OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                            {
                                OrderId = orderActionDTO.OrderId,
                                Status = "Cancelled",
                                ChangeBy = orderActionDTO.AccountId,
                                ChangeAt = DateTime.UtcNow,
                            };
                            await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                        }
                        var OrderRes = await (from o in _context.OrderrModel
                                              join h in _context.OrderStatusHistoryModel on o.Id equals h.OrderId
                                              join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                              where o.Id == orderActionDTO.OrderId
                                              select new OrderActionResDTO
                                              {
                                                  OrderId = o.Id,
                                                  SenderId = o.CustomerId,
                                                  ReceiverId = o.TechnicianId,
                                                  OrderName = o.Title,
                                                  CreatedAt = o.CreateAt,
                                              }).FirstOrDefaultAsync();
                        return OrderRes;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<OrderActionResDTO> ConfirmCompletedOrder(OrderActionDTO orderActionDTO)
        {
            {
                try
                {

                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            int isUpdated = await _context.OrderrModel.Where(o => o.Id == orderActionDTO.OrderId && o.Status == "In Progress").ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, "Confirmed"));
                            if (isUpdated > 0)
                            {
                                OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                                {
                                    OrderId = orderActionDTO.OrderId,
                                    Status = "Completed",
                                    ChangeBy = orderActionDTO.AccountId,
                                    ChangeAt = DateTime.UtcNow,
                                };
                                await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                                await _context.SaveChangesAsync();
                                await transaction.CommitAsync();
                            }
                            var OrderRes = await (from o in _context.OrderrModel
                                                  join h in _context.OrderStatusHistoryModel on o.Id equals h.OrderId
                                                  join c in _context.CustomerProfileModel on o.CustomerId equals c.Id
                                                  where o.Id == orderActionDTO.OrderId
                                                  select new OrderActionResDTO
                                                  {
                                                      OrderId = o.Id,
                                                      SenderId = o.CustomerId,
                                                      ReceiverId = o.TechnicianId,
                                                      OrderName = o.Title,
                                                      CreatedAt = o.CreateAt,
                                                  }).FirstOrDefaultAsync();
                            int isUpdateCount = await _context.TechnicianProfileModel.Where(t => t.Id == OrderRes.ReceiverId).ExecuteUpdateAsync(t => t.SetProperty(tp => tp.OrderCount, tp => tp.OrderCount + 1));
                            if(isUpdateCount == 0)
                            {
                                _logger.LogWarning("Failed to update order count for technician ID: {TechnicianId}", OrderRes.ReceiverId);
                                await transaction.RollbackAsync();
                                return null;
                            }
                            return OrderRes;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        }

        public async Task<List<OrderOverviewDTO>> GetCancalledOrder(Guid customerId)
        {
            try
            {
                List<OrderOverviewDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                               join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                               join c in _context.TechnicianProfileModel on o.TechnicianId equals c.Id
                                                               where o.CustomerId == customerId && o.Status == "Cancelled"
                                                               select new OrderOverviewDTO
                                                               {
                                                                   OrderId = o.Id,
                                                                   TechnicianName = c.FullName,
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

        public async Task<List<OrderOverviewDTO>> GetCurrentOrders(Guid customerId)
        {
            try
            {
                List<OrderOverviewDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                               join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                               join c in _context.TechnicianProfileModel on o.TechnicianId equals c.Id
                                                               where o.CustomerId == customerId &&
                                                                                                   (o.Status == "Pending Confirmation" ||
                                                                                                    o.Status == "Confirmed" ||
                                                                                                    o.Status == "In Progress")
                                                               select new OrderOverviewDTO
                                                               {
                                                                   OrderId = o.Id,
                                                                   TechnicianName = c.FullName,
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

        public Task<List<OrderOverviewDTO>> GetOrderDetail(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OrderOverviewDTO>> GetOrderHistory(Guid customerId)
        {
            try
            {
                List<OrderOverviewDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                               join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                               join c in _context.TechnicianProfileModel on o.TechnicianId equals c.Id
                                                               where o.CustomerId == customerId && o.Status == "Completed"
                                                               select new OrderOverviewDTO
                                                               {
                                                                   OrderId = o.Id,
                                                                   TechnicianName = c.FullName,
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

        public async Task<List<OrderOverviewDTO>> GetRejectedOrder(Guid customerId)
        {
            try
            {
                List<OrderOverviewDTO> InProgressOrder = await(from o in _context.OrderrModel
                                                               join s in _context.ServiceCategoriesModel on o.ServiceId equals s.Id
                                                               join c in _context.TechnicianProfileModel on o.TechnicianId equals c.Id
                                                               where o.CustomerId == customerId && o.Status == "Rejected"
                                                               select new OrderOverviewDTO
                                                               {
                                                                   OrderId = o.Id,
                                                                   TechnicianName = c.FullName,
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

        public async Task<bool> InsertOrder(CreateOrderDALDTO placeOrderDALDTO)
        {
            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        OrderrModel newOrder = new OrderrModel
                        {
                            CustomerId = placeOrderDALDTO.CustomerId,
                            TechnicianId = placeOrderDALDTO.TechnicianId,
                            ServiceId = placeOrderDALDTO.ServiceId,
                            Title = placeOrderDALDTO.Title,
                            Description = placeOrderDALDTO.Description,
                            Address = placeOrderDALDTO.Address,
                            City = placeOrderDALDTO.City,
                            Latitude = placeOrderDALDTO.Latitude,
                            Longitude = placeOrderDALDTO.Longitude,
                            CreateAt = DateTime.Now,
                            Status = "Pending Confirmation",
                        };
                        await _context.OrderrModel.AddAsync(newOrder);
                        await _context.SaveChangesAsync();
                        OrderStatusHistoryModel orderStatusHistory = new OrderStatusHistoryModel
                        {
                            OrderId = newOrder.Id,
                            Status = "Pending Confirmation",
                            ChangeBy = placeOrderDALDTO.CustomerId,
                            ChangeAt = DateTime.Now,
                        };
                        await _context.OrderStatusHistoryModel.AddAsync(orderStatusHistory);
                        await _context.SaveChangesAsync();
                       // Video
                       if(!string.IsNullOrEmpty(placeOrderDALDTO.videoUrl))
                        {
                            OrderAttachmentsModel videoAttachment = new OrderAttachmentsModel
                            {
                                OrderId = newOrder.Id,
                                FileType = "Video",
                                FileName = placeOrderDALDTO.videoUrl,
                            };
                            await _context.OrderAttachmentsModel.AddAsync(videoAttachment);
                            await _context.SaveChangesAsync();
                        }
                       // Images
                        if (placeOrderDALDTO.ImageOrderUrl != null && placeOrderDALDTO.ImageOrderUrl.Count > 0)
                        {
                            List<OrderAttachmentsModel> imageAttachments = placeOrderDALDTO.ImageOrderUrl.Select(imageUrl => new OrderAttachmentsModel
                            {
                                OrderId = newOrder.Id,
                                FileType = "Image",
                                FileName = imageUrl,
                            }).ToList();
                            await _context.OrderAttachmentsModel.AddRangeAsync(imageAttachments);
                            await _context.SaveChangesAsync();
                        }
                        await transaction.CommitAsync();
                        return true;
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

        public Task<OrderActionResDTO> updateOrder(OrderActionDTO orderActionDTO)
        {
            throw new NotImplementedException();
        }
    }

}