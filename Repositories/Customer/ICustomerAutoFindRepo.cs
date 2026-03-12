using Capstone_2_BE.DTOs.Customer.AutoFind;

namespace Capstone_2_BE.Repositories.Customer
{
    public interface ICustomerAutoFindRepo
    {
        Task<List<AutoFindFixerResDTO>> AutoFindCustomer(AutoFindFixerDTO autoFindFixerDTO);
    }
}
