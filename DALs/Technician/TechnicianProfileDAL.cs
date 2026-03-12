using Capstone_2_BE.DTOs.Technician.Profile;
using Capstone_2_BE.Repositories.Technician;
using Microsoft.EntityFrameworkCore;

namespace Capstone_2_BE.DALs.Technician
{
    public class TechnicianProfileDAL : ITechnicianProfileRepo
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;

        public TechnicianProfileDAL(AppDbContext context, ILogger<TechnicianProfileDAL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GetOldAvatar(Guid technicianId)
        {
            try
            {
                string? oldAvatar = await _context.TechnicianProfileModel
                    .Where(t => t.Id == technicianId)
                    .Select(t => t.AvatarURl)
                    .FirstOrDefaultAsync();
                return oldAvatar;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error retrieving old avatar: " + ex.Message);
                return null;
            }
             
        }

        public async Task<TechnicianProfileViewDTO> GetTechnicianProfile(Guid technicianId)
        {
            try
            {
                var technicianProfile = await _context.TechnicianProfileModel
                    .Where(t => t.Id == technicianId)
                    .Select(t => new TechnicianProfileViewDTO
                    {
                        
                        FullName = t.FullName,
                        AvatarURL = t.AvatarURl,
                        PhoneNumber = t.PhoneNumber,
                        Address = t.Address,
                        CreateAt = t.CreateAt,
                    })
                    .FirstOrDefaultAsync();
                var ServiceName = await (from t in _context.TechnicianProfileModel
                                         join sp in _context.Service_ProfileModel on t.Id equals sp.Id
                                         join s in _context.ServiceCategoriesModel on sp.ServiceId equals s.Id
                                         where t.Id == technicianId
                                         select s.ServiceName).FirstOrDefaultAsync();
                TechnicianProfileViewDTO result = new TechnicianProfileViewDTO
                {
                    FullName = technicianProfile.FullName,
                    AvatarURL = technicianProfile.AvatarURL,
                    PhoneNumber = technicianProfile.PhoneNumber,
                    Address = technicianProfile.Address,
                    City = technicianProfile.City,
                    CreateAt = technicianProfile.CreateAt,
                    ServiceName = ServiceName
                };
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error retrieving technician profile: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> UpdateTechnicianProfile(TechnicianProfileUpdateDALDTO technicianProfileUpdateDTO)
        {
            try
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                   int isUpdateInfor = await _context.TechnicianProfileModel.Where(t => t.Id == technicianProfileUpdateDTO.Id).ExecuteUpdateAsync(t => t
                        .SetProperty(p => p.FullName, technicianProfileUpdateDTO.FullName)
                        .SetProperty(p => p.PhoneNumber, technicianProfileUpdateDTO.PhoneNumber)
                        .SetProperty(p => p.Address, technicianProfileUpdateDTO.Address)
                        .SetProperty(p => p.City, technicianProfileUpdateDTO.City)
                        .SetProperty(p => p.Latitude, technicianProfileUpdateDTO.Latitude)
                        .SetProperty(p => p.Longitude, technicianProfileUpdateDTO.Longitude)
                    );
                    if (isUpdateInfor == 0)
                    {
                        _logger.LogWarning("No technician profile found with ID: {TechnicianId}", technicianProfileUpdateDTO.Id);
                        return false;
                    }
                    if(technicianProfileUpdateDTO.AvatarURl != null)
                    {
                        int isUpdateAvatar = await _context.TechnicianProfileModel.Where(a => a.Id == technicianProfileUpdateDTO.Id).ExecuteUpdateAsync(a => a
                            .SetProperty(p => p.AvatarURl, technicianProfileUpdateDTO.AvatarURl)
                        );
                        if (isUpdateAvatar == 0)
                        {
                            _logger.LogWarning("No account found with ID: {TechnicianId} for avatar update", technicianProfileUpdateDTO.Id);
                            return false;
                        }
                    }
                    if(technicianProfileUpdateDTO.ServiceId != Guid.Empty)
                    {
                        int isUpdateService = await _context.Service_ProfileModel.Where(s => s.Id == technicianProfileUpdateDTO.Id && s.ServiceId == technicianProfileUpdateDTO.ServiceId).ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.ServiceId, technicianProfileUpdateDTO.ServiceId)
                        );
                    }
                    await transaction.CommitAsync();
                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating technician profile: " + ex.Message);
                return false;
            }
        }
    }
}
