using Capstone_2_BE.DTOs.Technician.Orders;
using Capstone_2_BE.Repositories;

namespace Capstone_2_BE.Services.Technician
{
    public class TechnicianOrderService
    {
        private readonly ITechnicianOrderRepo _technicianOrderRepo;
        private readonly ILogger<TechnicianOrderService> _logger;

        public TechnicianOrderService(ITechnicianOrderRepo technicianOrderRepo, ILogger<TechnicianOrderService> logger)
        {
            _technicianOrderRepo = technicianOrderRepo;
            _logger = logger;
        }

        /// <summary>
        /// L?y t?ng quan t?t c? ??n hàng c?a k? thu?t viên
        /// </summary>
        public async Task<Result<TechnicianOrdersOverviewDTO>> GetOrdersOverview(Guid technicianId)
        {
            try
            {
                var inProgressOrder = await _technicianOrderRepo.GetInProgressOrders(technicianId);
                var confirmingOrders = await _technicianOrderRepo.GetConfirmingOrders(technicianId);
                var confirmedOrders = await _technicianOrderRepo.GetConfirmedOrders(technicianId);
                var historyOrders = await _technicianOrderRepo.GetHistoryOrders(technicianId);
                var canceledOrders = await _technicianOrderRepo.GetCanceledOrders(technicianId);

                var overview = new TechnicianOrdersOverviewDTO
                {
                    InProgressOrder = inProgressOrder,
                    ConfirmingOrders = confirmingOrders ?? new List<ViewOrderDTO>(),
                    ConfirmedOrders = confirmedOrders ?? new List<ViewOrderDTO>(),
                    TotalHistoryOrders = historyOrders?.Count ?? 0,
                    TotalCanceledOrders = canceledOrders?.Count ?? 0
                };

                return Result<TechnicianOrdersOverviewDTO>.Success(overview, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders overview for technician ID: {TechnicianId}", technicianId);
                return Result<TechnicianOrdersOverviewDTO>.Failure("L?i khi l?y t?ng quan ??n hàng", 500);
            }
        }

        /// <summary>
        /// L?y ??n hàng ?ang th?c hi?n
        /// </summary>
        public async Task<Result<ViewOrderDTO>> GetInProgressOrder(Guid technicianId)
        {
            try
            {
                var order = await _technicianOrderRepo.GetInProgressOrders(technicianId);
                
                if (order == null)
                {
                    return Result<ViewOrderDTO>.Failure("Không có ??n hàng ?ang th?c hi?n", 404);
                }

                return Result<ViewOrderDTO>.Success(order, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting in-progress order for technician ID: {TechnicianId}", technicianId);
                return Result<ViewOrderDTO>.Failure("L?i khi l?y ??n hàng ?ang th?c hi?n", 500);
            }
        }

        /// <summary>
        /// L?y danh sách ??n hàng ch? xác nh?n
        /// </summary>
        public async Task<Result<List<ViewOrderDTO>>> GetConfirmingOrders(Guid technicianId)
        {
            try
            {
                var orders = await _technicianOrderRepo.GetConfirmingOrders(technicianId);
                
                if (orders == null || orders.Count == 0)
                {
                    return Result<List<ViewOrderDTO>>.Success(new List<ViewOrderDTO>(), 200);
                }

                return Result<List<ViewOrderDTO>>.Success(orders, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirming orders for technician ID: {TechnicianId}", technicianId);
                return Result<List<ViewOrderDTO>>.Failure("L?i khi l?y danh sách ??n hàng ch? xác nh?n", 500);
            }
        }

        /// <summary>
        /// L?y danh sách ??n hàng ?ã xác nh?n
        /// </summary>
        public async Task<Result<List<ViewOrderDTO>>> GetConfirmedOrders(Guid technicianId)
        {
            try
            {
                var orders = await _technicianOrderRepo.GetConfirmedOrders(technicianId);
                
                if (orders == null || orders.Count == 0)
                {
                    return Result<List<ViewOrderDTO>>.Success(new List<ViewOrderDTO>(), 200);
                }

                return Result<List<ViewOrderDTO>>.Success(orders, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmed orders for technician ID: {TechnicianId}", technicianId);
                return Result<List<ViewOrderDTO>>.Failure("L?i khi l?y danh sách ??n hàng ?ã xác nh?n", 500);
            }
        }

        /// <summary>
        /// L?y l?ch s? ??n hàng ?ã hoàn thành
        /// </summary>
        public async Task<Result<List<ViewOrderDTO>>> GetHistoryOrders(Guid technicianId)
        {
            try
            {
                var orders = await _technicianOrderRepo.GetHistoryOrders(technicianId);
                
                if (orders == null || orders.Count == 0)
                {
                    return Result<List<ViewOrderDTO>>.Success(new List<ViewOrderDTO>(), 200);
                }

                return Result<List<ViewOrderDTO>>.Success(orders, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history orders for technician ID: {TechnicianId}", technicianId);
                return Result<List<ViewOrderDTO>>.Failure("L?i khi l?y l?ch s? ??n hàng", 500);
            }
        }

        /// <summary>
        /// L?y danh sách ??n hàng ?ã h?y
        /// </summary>
        public async Task<Result<List<ViewOrderDTO>>> GetCanceledOrders(Guid technicianId)
        {
            try
            {
                var orders = await _technicianOrderRepo.GetCanceledOrders(technicianId);
                
                if (orders == null || orders.Count == 0)
                {
                    return Result<List<ViewOrderDTO>>.Success(new List<ViewOrderDTO>(), 200);
                }

                return Result<List<ViewOrderDTO>>.Success(orders, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting canceled orders for technician ID: {TechnicianId}", technicianId);
                return Result<List<ViewOrderDTO>>.Failure("L?i khi l?y danh sách ??n hàng ?ã h?y", 500);
            }
        }

        /// <summary>
        /// Xác nh?n ??n hàng (Pending Confirmation -> Confirmed)
        /// </summary>
        public async Task<Result<string>> ConfirmOrder(OrderActionDTO orderActionDTO)
        {
            try
            {
                var result = await _technicianOrderRepo.ConfirmOrder(orderActionDTO.OrderId, orderActionDTO.AccountId);
                
                if (result)
                {
                    return Result<string>.Success("Xác nh?n ??n hàng thành công", 200);
                }
                else
                {
                    return Result<string>.Failure("Không th? xác nh?n ??n hàng. ??n hàng không t?n t?i ho?c không ? tr?ng thái ch? xác nh?n", 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order ID: {OrderId}", orderActionDTO.OrderId);
                return Result<string>.Failure("L?i khi xác nh?n ??n hàng", 500);
            }
        }

        /// <summary>
        /// B?t ??u th?c hi?n ??n hàng (Confirmed -> In Progress)
        /// </summary>
        public async Task<Result<string>> StartOrder(OrderActionDTO orderActionDTO)
        {
            try
            {
                // Ki?m tra xem có ??n hàng ?ang th?c hi?n không
                var inProgressOrder = await _technicianOrderRepo.GetInProgressOrders(orderActionDTO.AccountId);
                if (inProgressOrder != null)
                {
                    return Result<string>.Failure("B?n ?ang có m?t ??n hàng ?ang th?c hi?n. Vui lòng hoàn thành ??n hàng hi?n t?i tr??c", 400);
                }

                var result = await _technicianOrderRepo.StartOrder(orderActionDTO.OrderId, orderActionDTO.AccountId);
                
                if (result)
                {
                    return Result<string>.Success("B?t ??u th?c hi?n ??n hàng thành công", 200);
                }
                else
                {
                    return Result<string>.Failure("Không th? b?t ??u ??n hàng. ??n hàng không t?n t?i ho?c không ? tr?ng thái ?ã xác nh?n", 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting order ID: {OrderId}", orderActionDTO.OrderId);
                return Result<string>.Failure("L?i khi b?t ??u th?c hi?n ??n hàng", 500);
            }
        }

        /// <summary>
        /// Hoàn thành ??n hàng (In Progress -> Completed)
        /// </summary>
        public async Task<Result<string>> CompleteOrder(OrderActionDTO orderActionDTO)
        {
            try
            {
                var result = await _technicianOrderRepo.CompleteOrder(orderActionDTO.OrderId, orderActionDTO.AccountId);
                
                if (result)
                {
                    return Result<string>.Success("Hoàn thành ??n hàng thành công", 200);
                }
                else
                {
                    return Result<string>.Failure("Không th? hoàn thành ??n hàng. ??n hàng không t?n t?i ho?c không ? tr?ng thái ?ang th?c hi?n", 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing order ID: {OrderId}", orderActionDTO.OrderId);
                return Result<string>.Failure("L?i khi hoàn thành ??n hàng", 500);
            }
        }

        /// <summary>
        /// H?y ??n hàng (Pending Confirmation -> Refuse)
        /// </summary>
        public async Task<Result<string>> CancelOrder(OrderActionDTO orderActionDTO)
        {
            try
            {
                var result = await _technicianOrderRepo.CancelOrder(orderActionDTO.OrderId, orderActionDTO.AccountId);
                
                if (result)
                {
                    return Result<string>.Success("H?y ??n hàng thành công", 200);
                }
                else
                {
                    return Result<string>.Failure("Không th? h?y ??n hàng. ??n hàng không t?n t?i ho?c không ? tr?ng thái ch? xác nh?n", 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order ID: {OrderId}", orderActionDTO.OrderId);
                return Result<string>.Failure("L?i khi h?y ??n hàng", 500);
            }
        }
    }
}
