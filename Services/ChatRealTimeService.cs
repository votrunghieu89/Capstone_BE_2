using Capstone_2_BE.DTOs.ChatRealTime;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Settings;
using Capstone_2_BE.Socket;
using Microsoft.AspNetCore.SignalR;

namespace Capstone_2_BE.Services
{
    public class ChatRealTimeService
    {
        private readonly IChatRealTimeRepo _chatRealTimeRepository;
        private readonly ILogger<ChatRealTimeService> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly AWS _aws;

        public ChatRealTimeService(IChatRealTimeRepo chatRealTimeRepository, ILogger<ChatRealTimeService> logger, IHubContext<ChatHub> chatHubContext, AWS aws)
        {
            _chatRealTimeRepository = chatRealTimeRepository;
            _logger = logger;
            _chatHubContext = chatHubContext;
            _aws = aws;
        }

        public async Task<Result<string>> MarkAsRead(Guid roomId, Guid AccountId)
        {
            try
            {
                bool success = await _chatRealTimeRepository.MarkAsRead(roomId, AccountId);
                if (success)
                {
                    return Result<string>.Success("Messages marked as read successfully.", 200);
                }
                else
                {
                    return Result<string>.Failure("Failed to mark messages as read.", 400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for room {RoomId} and account {AccountId}", roomId, AccountId);
                return Result<string>.Failure("An error occurred while marking messages as read.", 500);
            }
        }

        public async Task<Result<List<ViewAllRoomDTO>>> GetAllRooms(Guid AccountId, int page, int pageSize)
        {
            try
            {
                List<ViewAllRoomDTO> rooms = await _chatRealTimeRepository.GetAllRooms(AccountId, page, pageSize);
                foreach (var room in rooms)
                {
                    if (room.AvatarUrl != null)
                    {
                        room.AvatarUrl = await _aws.ReadImage(room.AvatarUrl);
                    }
                }
                if (rooms == null)
                {
                    rooms = new List<ViewAllRoomDTO>();
                }
                return Result<List<ViewAllRoomDTO>>.Success(rooms, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms for account {AccountId}", AccountId);
                return Result<List<ViewAllRoomDTO>>.Failure("An error occurred while retrieving rooms.", 500);
            }
        }

        public async Task<Result<List<ViewAllMessageDTO>>> GetAllMessages(Guid RoomId, int page, int pageSize)
        {
            try
            {
                List<ViewAllMessageDTO> messages = await _chatRealTimeRepository.GetAllMessages(RoomId, page, pageSize);
                foreach (var message in messages)
                {
                    if (message.AvatarUrl != null)
                    {
                        message.AvatarUrl = await _aws.ReadImage(message.AvatarUrl);
                    }
                    if (message.ImageUrls != null && message.ImageUrls.Count > 0)
                    {
                        for (int i = 0; i < message.ImageUrls.Count; i++)
                        {
                            message.ImageUrls[i] = await _aws.ReadImage(message.ImageUrls[i]);
                        }
                    }
                    if (message.videoUrl != null)
                    {
                        message.videoUrl = await _aws.ReadImage(message.videoUrl);
                    }
                }
                if (messages == null || messages.Count == 0)
                {
                    messages = new List<ViewAllMessageDTO>();
                }
                return Result<List<ViewAllMessageDTO>>.Success(messages, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for room {RoomId}", RoomId);
                return Result<List<ViewAllMessageDTO>>.Failure("An error occurred while retrieving messages.", 500);
            }
        }
        public async Task<Result<Guid>> GetorCreateRoom(Guid userA, Guid userB)
        {
            try
            {
                Guid roomId = await _chatRealTimeRepository.GetOrCreateRoom(userA, userB);
                if (roomId == Guid.Empty)
                {
                    return Result<Guid>.Failure("Failed to create or retrieve chat room.", 400);
                }
                return Result<Guid>.Success(roomId, 200);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or retrieving chat room for users {UserA} and {UserB}", userA, userB);
                return Result<Guid>.Failure("An error occurred while creating or retrieving the chat room.", 500);
            }
        }
        public async Task<Result<string>> InsertMessage(CreateMessageFormDTO createMessageFormDTO)
        {
            try
            {
                var normalizedContent = string.IsNullOrWhiteSpace(createMessageFormDTO.Content)
                    ? null
                    : createMessageFormDTO.Content.Trim();
                var hasImages = createMessageFormDTO.ImageUrls != null && createMessageFormDTO.ImageUrls.Count > 0;
                var hasVideo = createMessageFormDTO.VideoUrl != null;
                if (normalizedContent == null && !hasImages && !hasVideo)
                {
                    return Result<string>.Failure("Message content is empty.", 400);
                }

                Guid roomId = await _chatRealTimeRepository.GetOrCreateRoom(createMessageFormDTO.SenderId, createMessageFormDTO.ReceiverId);
                if (roomId == Guid.Empty)
                {
                    return Result<string>.Failure("Failed to create or retrieve chat room.", 400);
                }
                CreateMessageDTO createMessageDTO = new CreateMessageDTO
                {
                    RoomId = roomId,
                    SenderId = createMessageFormDTO.SenderId,
                    Content = normalizedContent ?? "[media]",
                    ImageUrls = new List<string>(),
                    VideoUrl = null
                };
                if (createMessageFormDTO.ImageUrls != null && createMessageFormDTO.ImageUrls.Count > 0)
                {
                    foreach (var image in createMessageFormDTO.ImageUrls)
                    {
                        string? imageUrl = await _aws.UploadChatImage(image);
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            createMessageDTO.ImageUrls.Add(imageUrl);
                        }
                    }
                    if (hasImages && createMessageDTO.ImageUrls.Count == 0)
                    {
                        return Result<string>.Failure("Image upload failed.", 400);
                    }
                }
                if (createMessageFormDTO.VideoUrl != null)
                {
                    string? videoUrl = await _aws.UploadChatVideo(createMessageFormDTO.VideoUrl);
                    if (string.IsNullOrWhiteSpace(videoUrl))
                    {
                        return Result<string>.Failure("Video upload failed.", 400);
                    }
                    createMessageDTO.VideoUrl = videoUrl;
                }

                var Response = await _chatRealTimeRepository.InsertMessage(createMessageDTO);
                if (Response == null)
                {
                    return Result<string>.Failure("Failed to save message.", 500);
                }

                string? avatarUrl = null;
                if (!string.IsNullOrWhiteSpace(Response.AvatarUrl))
                {
                    avatarUrl = await _aws.ReadImage(Response.AvatarUrl);
                }
                // Gửi tin nhắn khi 2 người trong đoạn chat
                await _chatHubContext.Clients.Group(roomId.ToString())
                                                .SendAsync("ChatMessage", new
                                                {
                                                    MessId = Response.MessengerId,
                                                    RoomId = roomId,
                                                    SenderId = createMessageFormDTO.SenderId,
                                                    Content = createMessageDTO.Content,
                                                    AvatarUrl = avatarUrl,
                                                    ImageUrls = createMessageDTO.ImageUrls.Count > 0 ? createMessageDTO.ImageUrls : null,
                                                    VideoUrl = createMessageDTO.VideoUrl,
                                                    CreatedAt = DateTime.Now
                                                });
                // Tạo icon bên phía UserB khi User A chủ động nhắn, và đẩy icon  tin nhắn lên đầu Chat Sidebar
                await _chatHubContext.Clients.User(createMessageFormDTO.ReceiverId.ToString())
                                                    .SendAsync("NewMessageNotification", new
                                                    {
                                                        MessId = Response.MessengerId,
                                                        RoomId = roomId,
                                                        SenderId = createMessageFormDTO.SenderId,
                                                        Content = createMessageDTO.Content,
                                                        AvatarUrl = avatarUrl,
                                                        ImageUrls = createMessageDTO.ImageUrls.Count > 0 ? createMessageDTO.ImageUrls : null,
                                                        VideoUrl = createMessageDTO.VideoUrl,
                                                        CreatedAt = DateTime.Now
                                                    });
                return Result<string>.Success("Message sent successfully.", 200);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting chat message from {SenderId} to {ReceiverId}", createMessageFormDTO.SenderId, createMessageFormDTO.ReceiverId);
                return Result<string>.Failure("An error occurred while sending the message.", 500);
            }
        }
    }
}