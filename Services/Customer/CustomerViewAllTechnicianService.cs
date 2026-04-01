using Capstone_2_BE.DTOs.Customer.FindTechnician;
using Capstone_2_BE.DTOs.Customer.Order;
using Capstone_2_BE.Repositories.Customer;
using Capstone_2_BE.Settings;

namespace Capstone_2_BE.Services.Customer
{
    public class CustomerViewAllTechnicianService
    {
        private readonly ICustomerViewAllTechnicianRepo _repo;
        private readonly AWS _aws;
        private readonly ILogger<CustomerViewAllTechnicianService> _logger;

        public CustomerViewAllTechnicianService(ICustomerViewAllTechnicianRepo repo, AWS aws, ILogger<CustomerViewAllTechnicianService> logger)
        {
            _repo = repo;
            _aws = aws;
            _logger = logger;
        }

        public async Task<Result<List<ViewAllTechnicianDTO>>> ViewAllTechnician()
        {
            try
            {
                var list = await _repo.ViewALLTechnician();
                if (list == null)
                    return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?y danh sách k? thu?t viên", 500);

                // convert avatar keys to public urls
                foreach (var t in list)
                {
                    if (!string.IsNullOrEmpty(t.AvatarUrl))
                    {
                        try { t.AvatarUrl = await _aws.ReadImage(t.AvatarUrl); } catch { /* ignore */ }
                    }
                }

                return Result<List<ViewAllTechnicianDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ViewAllTechnician");
                return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?y danh sách k? thu?t viên", 500);
            }
        }

        public async Task<Result<List<ViewAllTechnicianDTO>>> FilterByArea(Guid cityId)
        {
            try
            {
                var list = await _repo.FilterTechnicianbyArea(cityId);
                if (list == null) return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?c theo khu v?c", 500);
                foreach (var t in list) if (!string.IsNullOrEmpty(t.AvatarUrl)) { try { t.AvatarUrl = await _aws.ReadImage(t.AvatarUrl); } catch { } }
                return Result<List<ViewAllTechnicianDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterByArea for city {City}", cityId);
                return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?c theo khu v?c", 500);
            }
        }

        public async Task<Result<List<ViewAllTechnicianDTO>>> FilterTechnicianCombination(TechnicianFilterRequestDTO filter)
        {
            try
            {
                // 1. Validation
                if (filter == null)
                    return Result<List<ViewAllTechnicianDTO>>.Failure("Dữ liệu lọc không hợp lệ", 400);

                if (filter.startRate.HasValue && (filter.startRate < 0 || filter.startRate > 5))
                    return Result<List<ViewAllTechnicianDTO>>.Failure("Rating phải từ 0 đến 5", 400);

                if (filter.endRate.HasValue && (filter.endRate < 0 || filter.endRate > 5))
                    return Result<List<ViewAllTechnicianDTO>>.Failure("Rating phải từ 0 đến 5", 400);

                if (filter.startRate.HasValue && filter.endRate.HasValue && filter.startRate > filter.endRate)
                    return Result<List<ViewAllTechnicianDTO>>.Failure("startRate không được lớn hơn endRate", 400);

                // 2. Call repo (🔥 ĐÚNG THỨ TỰ)
                var list = await _repo.FilterTechnicians(
                    filter.startRate,
                    filter.endRate,
                    filter.CityId,
                    filter.ServiceId
                );

                if (list == null)
                    return Result<List<ViewAllTechnicianDTO>>.Failure("Lỗi khi lọc kỹ thuật viên", 500);

                // 3. Xử lý avatar (song song)
                var imageTasks = list
                    .Where(t => !string.IsNullOrEmpty(t.AvatarUrl))
                    .Select(async t =>
                    {
                        try
                        {
                            t.AvatarUrl = await _aws.ReadImage(t.AvatarUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Không thể đọc ảnh cho kỹ thuật viên {Id}: {Msg}", t.TechnicianId, ex.Message);
                            t.AvatarUrl = "default-avatar-url";
                        }
                    });

                await Task.WhenAll(imageTasks);

                return Result<List<ViewAllTechnicianDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterTechnicianCombination with criteria: {@Filter}", filter);
                return Result<List<ViewAllTechnicianDTO>>.Failure("Hệ thống gặp lỗi khi xử lý lọc", 500);
            }
        }
        public async Task<Result<List<ViewAllTechnicianDTO>>> FilterByService(Guid serviceId)
        {
            try
            {
                var list = await _repo.FilterTechnicianbyService(serviceId);
                if (list == null) return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?c theo d?ch v?", 500);
                foreach (var t in list) if (!string.IsNullOrEmpty(t.AvatarUrl)) { try { t.AvatarUrl = await _aws.ReadImage(t.AvatarUrl); } catch { } }
                return Result<List<ViewAllTechnicianDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterByService for service {ServiceId}", serviceId);
                return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?c theo d?ch v?", 500);
            }
        }

        public async Task<Result<List<ViewAllTechnicianDTO>>> FilterByRate(decimal startRate, decimal endRate)
        {
            try
            {
                var list = await _repo.FilterTechnicianbyRate(startRate, endRate);
                if (list == null) return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?c theo ?ánh giá", 500);
                foreach (var t in list) if (!string.IsNullOrEmpty(t.AvatarUrl)) { try { t.AvatarUrl = await _aws.ReadImage(t.AvatarUrl); } catch { } }
                return Result<List<ViewAllTechnicianDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterByRate {Start}-{End}", startRate, endRate);
                return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi l?c theo ?ánh giá", 500);
            }
        }

        public async Task<Result<List<ViewAllTechnicianDTO>>> SearchByName(string name)
        {
            try
            {
                var list = await _repo.SearchTechnicianbyName(name);
                if (list == null) return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi tìm ki?m theo tên", 500);
                foreach (var t in list) if (!string.IsNullOrEmpty(t.AvatarUrl)) { try { t.AvatarUrl = await _aws.ReadImage(t.AvatarUrl); } catch { } }
                return Result<List<ViewAllTechnicianDTO>>.Success(list, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchByName {Name}", name);
                return Result<List<ViewAllTechnicianDTO>>.Failure("L?i khi tìm ki?m theo tên", 500);
            }
        }

        public async Task<Result<bool>> PlaceOrder(CreateOrderFormDTO form)
        {
            try
            {
                var dalDto = new CreateOrderDALDTO
                {
                    CustomerId = form.CustomerId,
                    TechnicianId = form.TechnicianId,
                    ServiceId = form.ServiceId,
                    Title = form.Title,
                    Description = form.Description,
                    Address = form.Address,
                    CityId = form.CityId,
                    Latitude = form.Latitude,
                    Longitude = form.Longitude,
                    ImageOrderUrl = new List<string>(),
                    videoUrl = string.Empty
                };

                if (form.VideoFile != null)
                {
                    var videoKey = await _aws.UploadVideoOrder(form.VideoFile);
                    if (string.IsNullOrEmpty(videoKey)) return Result<bool>.Failure("Upload video th?t b?i", 400);
                    dalDto.videoUrl = videoKey;
                }

                if (form.ImageFiles != null && form.ImageFiles.Count > 0)
                {
                    foreach (var file in form.ImageFiles)
                    {
                        var key = await _aws.UploadImageOrder(file);
                        if (!string.IsNullOrEmpty(key)) dalDto.ImageOrderUrl.Add(key);
                    }
                }

                var ok = await _repo.PlaceOrderForTechnician(dalDto);
                if (ok) return Result<bool>.Success(true, 200);
                return Result<bool>.Failure("??t ??n cho k? thu?t viên th?t b?i", 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order for technician {TechnicianId}", form.TechnicianId);
                return Result<bool>.Failure("L?i khi ??t ??n", 500);
            }
        }
    }
}
