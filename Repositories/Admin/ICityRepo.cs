using Capstone_2_BE.DTOs.City;
using Capstone_2_BE.DTOs.City;
using Capstone_2_BE.Models;

namespace Capstone_2_BE.Repositories.Administrator
{
    public interface ICityRepo
    {
        Task<bool> CreateCity(string cityName);
        Task<bool> UpdateCity(Guid cityId, string cityName);
        Task<bool> DeleteCity(Guid cityId);
        Task<List<ViewAllCities>> ViewAllCities();
        Task<string> GetCityName(Guid CityId);
        Task<Guid> GetCityID(string cityName);
    } 
}
     
