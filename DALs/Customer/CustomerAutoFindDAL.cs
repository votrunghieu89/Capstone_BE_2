using Capstone_2_BE.DTOs.Customer.AutoFind;
using Capstone_2_BE.Repositories.Customer;
using Microsoft.EntityFrameworkCore;

namespace Capstone_2_BE.DALs.Customer
{
    public class CustomerAutoFindDAL : ICustomerAutoFindRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerAutoFindDAL> _logger;

        public CustomerAutoFindDAL(AppDbContext context, ILogger<CustomerAutoFindDAL> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<AutoFindFixerResDTO>> AutoFindCustomer(AutoFindFixerDTO autoFindFixerDTO)
        {
            try
            {
                var TechList = await (from a in _context.AccountsModel
                                                        join t in _context.TechnicianProfileModel on a.Id equals t.Id
                                                        join sp in _context.Service_ProfileModel on t.Id equals sp.TechnicianId
                                                        join sc in _context.ServiceCategoriesModel on sp.ServiceId equals sc.Id
                                                        where a.IsOnline == 1 && t.City == autoFindFixerDTO.City && sp.ServiceId == autoFindFixerDTO.ServiceId
                                                        select new
                                                        {
                                                            TechnicianId = t.Id,
                                                            FullName = t.FullName,
                                                            AvatarURL = t.AvatarURl,
                                                            ServiceName = sc.ServiceName,
                                                            Latitude = t.Latitude,
                                                            Longitude = t.Longitude
                                                        }).ToListAsync();
                var result =  new List<AutoFindFixerResDTO>();
                foreach (var tech in TechList)
                {
                    var score = await _context.RatingModel.Where(r => r.TechnicianId == tech.TechnicianId).AverageAsync(r => (decimal?)r.Score) ?? 0;
                    result.Add(new AutoFindFixerResDTO
                    {
                        TechnicianId = tech.TechnicianId,
                        FullName = tech.FullName,
                        avatarURL = tech.AvatarURL,
                        ServiceName = tech.ServiceName,
                        Latitude = tech.Latitude,
                        Longitude = tech.Longitude,
                        Score = score,
                        Total = score
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
