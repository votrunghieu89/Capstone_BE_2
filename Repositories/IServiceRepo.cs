using Capstone_2_BE.DTOs.Service;

namespace Capstone_2_BE.Repositories
{
    public interface IServiceRepo
    {
        Task<string?> GetServiceName(Guid serviceId);
        Task<List<ServiceDTO>> GetAllServices();
        Task<Guid?> GetServiceIdByName(string serviceName);
    }
}
