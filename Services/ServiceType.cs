using Capstone_2_BE.DTOs.Service;
using Capstone_2_BE.Repositories;

namespace Capstone_2_BE.Services
{
    public class ServiceType
    {
        private readonly IServiceRepo _serviceRepo;
        private readonly ILogger<ServiceType> _logger;

        public ServiceType(IServiceRepo serviceRepo, ILogger<ServiceType> logger)
        {
            _serviceRepo = serviceRepo;
            _logger = logger;
        }

        public async Task<Result<string>> GetServiceName(Guid serviceId)
        {
            try
            {
                var name = await _serviceRepo.GetServiceName(serviceId);
                if (string.IsNullOrEmpty(name)) return Result<string>.Failure("Service not found", 404);
                return Result<string>.Success(name, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service name for {ServiceId}", serviceId);
                return Result<string>.Failure("Error retrieving service name", 500);
            }
        }

        public async Task<Result<List<ServiceDTO>>> GetAllServices()
        {
            try
            {
                var list = await _serviceRepo.GetAllServices();
                return Result<List<ServiceDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all services");
                return Result<List<ServiceDTO>>.Failure("Error retrieving services", 500);
            }
        }

        public async Task<Result<Guid>> GetServiceIdByName(string serviceName)
        {
            try
            {
                var id = await _serviceRepo.GetServiceIdByName(serviceName);
                if (!id.HasValue) return Result<Guid>.Failure("Service not found", 404);
                return Result<Guid>.Success(id.Value, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service id for {ServiceName}", serviceName);
                return Result<Guid>.Failure("Error retrieving service id", 500);
            }
        }
    }
}
