using Capstone_2_BE.DTOs.Technician.Rating;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Settings;

namespace Capstone_2_BE.Services.Technician
{
    public class TechnicianRatingService
    {
        private readonly ITechnicianRatingRepo _technicianRatingRepo;
        private readonly AWS _aws;
        private readonly ILogger<TechnicianRatingService> _logger;

        public TechnicianRatingService(ITechnicianRatingRepo technicianRatingRepo, AWS aws, ILogger<TechnicianRatingService> logger)
        {
            _technicianRatingRepo = technicianRatingRepo;
            _aws = aws;
            _logger = logger;
        }

        public async Task<Result<TechnicianRatingViewDTO>> GetTechnicianRatingOverview(Guid technicianId)
        {
            try
            {
                var ratingOverview = await _technicianRatingRepo.getTechniqueRateOverview(technicianId);
                
                if (ratingOverview == null)
                {
                    return Result<TechnicianRatingViewDTO>.Failure("Không tìm thấy thông tin đánh giá kỹ thuật viên", 404);
                }

                // Convert avatar key to public URL if exists
                if (!string.IsNullOrEmpty(ratingOverview.AvatarURL))
                {
                    ratingOverview.AvatarURL = await _aws.ReadImage(ratingOverview.AvatarURL);
                }

                return Result<TechnicianRatingViewDTO>.Success(ratingOverview, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting technician rating overview for ID: {TechnicianId}", technicianId);
                return Result<TechnicianRatingViewDTO>.Failure("Lỗi khi lấy thông tin đánh giá kỹ thuật viên", 500);
            }
        }

        public async Task<Result<List<TechnicianFeedbackViewDTO>>> GetTechnicianFeedbacks(Guid technicianId)
        {
            try
            {
                var feedbacks = await _technicianRatingRepo.getTechniqueFeedBack(technicianId);
                
                if (feedbacks == null || feedbacks.Count == 0)
                {
                    return Result<List<TechnicianFeedbackViewDTO>>.Success(new List<TechnicianFeedbackViewDTO>(), 200);
                }

                // Convert all customer avatar keys to public URLs
                foreach (var feedback in feedbacks)
                {
                    if (!string.IsNullOrEmpty(feedback.CustomerAvatarURL))
                    {
                        feedback.CustomerAvatarURL = await _aws.ReadImage(feedback.CustomerAvatarURL);
                    }
                }

                return Result<List<TechnicianFeedbackViewDTO>>.Success(feedbacks, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting technician feedbacks for ID: {TechnicianId}", technicianId);
                return Result<List<TechnicianFeedbackViewDTO>>.Failure("Lỗi khi lấy danh sách đánh giá", 500);
            }
        }

        public async Task<Result<TechnicianRatingDetailDTO>> GetTechnicianRatingDetail(Guid technicianId)
        {
            try
            {
                // Get rating overview
                var overviewResult = await GetTechnicianRatingOverview(technicianId);
                if (!overviewResult.IsSuccess)
                {
                    return Result<TechnicianRatingDetailDTO>.Failure(overviewResult.Error, overviewResult.StatusCode);
                }

                // Get feedbacks
                var feedbacksResult = await GetTechnicianFeedbacks(technicianId);
                if (!feedbacksResult.IsSuccess)
                {
                    return Result<TechnicianRatingDetailDTO>.Failure(feedbacksResult.Error, feedbacksResult.StatusCode);
                }

                var detail = new TechnicianRatingDetailDTO
                {
                    Id = overviewResult.Data.Id,
                    FullName = overviewResult.Data.FullName,
                    AvatarURL = overviewResult.Data.AvatarURL,
                    AverageScore = overviewResult.Data.Score,
                    TotalFeedbacks = feedbacksResult.Data.Count,
                    Feedbacks = feedbacksResult.Data
                };

                return Result<TechnicianRatingDetailDTO>.Success(detail, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting technician rating detail for ID: {TechnicianId}", technicianId);
                return Result<TechnicianRatingDetailDTO>.Failure("Lỗi khi lấy chi tiết đánh giá kỹ thuật viên", 500);
            }
        }
    }
}
