using Capstone_2_BE.DTOs.Technician.Orders;

namespace Capstone_2_BE.Repositories
{
    public interface ITechnicianOrderRepo
    {
        // View in Progress Orders
        Task<ViewOrderDTO> GetInProgressOrders(Guid technicianId);
        // View Cofirming Orders
        Task<List<ViewOrderDTO>> GetConfirmingOrders(Guid technicianId);
        // View Comfimred Orders
        Task<List<ViewOrderDTO>> GetConfirmedOrders(Guid technicianId);
        // View History Orders
        Task<List<ViewOrderDTO>> GetHistoryOrders(Guid technicianId);
        // View Canceled Orders
        Task<List<ViewOrderDTO>> GetCanceledOrders(Guid technicianId);
        // View Order Details
        // Change Confirming Order to Confirmed
        Task<bool> ConfirmOrder(Guid orderId, Guid AccountId);
        // Changge Confirmed Order to In Progress
        Task<bool> StartOrder(Guid orderId, Guid AccountId);
        // Change In Progress Order to Completed
        Task<bool> CompleteOrder(Guid orderId, Guid AccountId);
        // Cancel Order
        Task<bool> CancelOrder(Guid orderId, Guid AccountId);
    }
}
