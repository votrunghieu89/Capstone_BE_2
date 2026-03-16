using Capstone_2_BE.DTOs.Service;
using Capstone_2_BE.Models;
using Capstone_2_BE.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Capstone_2_BE.DALs
{
    public class ServiceDAL : IServiceRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ServiceDAL> _logger;

        public ServiceDAL(AppDbContext context, ILogger<ServiceDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetServiceName(Guid serviceId)
        {
            try
            {
                var svc = await _context.ServiceCategoriesModel.Where(s => s.Id == serviceId).Select(s => s.ServiceName).FirstOrDefaultAsync();
                return svc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service name for {ServiceId}", serviceId);
                return null;
            }
        }

        public async Task<List<ServiceDTO>> GetAllServices()
        {
            try
            {
                return await _context.ServiceCategoriesModel
                    .Select(s => new ServiceDTO { Id = s.Id, ServiceName = s.ServiceName })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all services");
                return new List<ServiceDTO>();
            }
        }

        public async Task<Guid?> GetServiceIdByName(string serviceName)
        {
            try
            {
               var serviceId = await _context.ServiceCategoriesModel
                    .Where(s => s.ServiceName == serviceName)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();
                if (serviceId == Guid.Empty)
                {
                    _logger.LogWarning("Service name {ServiceName} not found", serviceName);
                    return null;
                }
                return serviceId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service id for {ServiceName}", serviceName);
                return null;
            }
        }
    }
}
