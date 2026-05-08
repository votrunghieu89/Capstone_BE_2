using Capstone_2_BE.DTOs.Invoices;

namespace Capstone_2_BE.Repositories
{
    public interface IInvoiceRepo
    {
        Task<bool> checkIsInvoice(Guid OrderId);
        Task<List<ViewAllCompletedOrderDTO>> getAllCompletedOrder(Guid TechnicianId);
        Task<List<ViewAllCompletedOrderforCustomerDTO>> getAllCompletedOrderforCustomer(Guid CustomerId);
        Task<bool> CreateInvoice(CreateInvoiceDTO create);
        Task<bool> DeleteInvoice(Guid InvoiceId);
        Task<ViewDetailInvoiceDTO> GetDetailInvoicebyOrderId(Guid OrderId);
        Task<bool> IsPayment(Guid InvoiceId);
        Task<bool> ConfirmPayment (Guid InvoiceId);
        Task<List<ViewAllInvoice>> GetAllInvoice();
        Task<ViewUpdateInvoiceDTO> GetInvoiceItemforUpdate(Guid OrderId);
        Task<bool> UpdateInvoice(Guid OrderId, CreateInvoiceDTO createInvoiceDTO);
        
    }
}
