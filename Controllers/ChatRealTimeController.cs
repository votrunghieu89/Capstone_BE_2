using Capstone_2_BE.DTOs.ChatRealTime;
using Capstone_2_BE.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Capstone_2_BE.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatRealTimeController : ControllerBase
    {
        private readonly ChatRealTimeService _chatService;
        private readonly ILogger<ChatRealTimeController> _logger;

        public ChatRealTimeController(ChatRealTimeService chatService, ILogger<ChatRealTimeController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromQuery] Guid roomId, [FromQuery] Guid accountId)
        {
            var result = await _chatService.MarkAsRead(roomId, accountId);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { message = result.Data });
        }

        [HttpGet("rooms/{accountId}")]
        public async Task<IActionResult> GetAllRooms(Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _chatService.GetAllRooms(accountId, page, pageSize);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpGet("messages/{roomId}")]
        public async Task<IActionResult> GetAllMessages(Guid roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _chatService.GetAllMessages(roomId, page, pageSize);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, result.Data);
        }

        [HttpPost("room")]
        public async Task<IActionResult> GetOrCreateRoom([FromQuery] Guid userA, [FromQuery] Guid userB)
        {
            var result = await _chatService.GetorCreateRoom(userA, userB);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { roomId = result.Data });
        }

        [HttpPost("message")]
        [Consumes("application/json", "multipart/form-data")]
        [RequestSizeLimit(104_857_600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
        public async Task<IActionResult> InsertMessage()
        {
            var createMessageFormDTO = await BindCreateMessageFromRequest();
            if (createMessageFormDTO is null)
            {
                return BadRequest(new { message = "Payload không hợp lệ." });
            }

            var result = await _chatService.InsertMessage(createMessageFormDTO);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, new { message = result.Error });
            return StatusCode(result.StatusCode, new { message = result.Data });
        }

        private async Task<CreateMessageFormDTO?> BindCreateMessageFromRequest()
        {
            try
            {
                if (Request.HasFormContentType)
                {
                    var form = await Request.ReadFormAsync();

                    var senderRaw = form["SenderId"].FirstOrDefault() ?? form["senderId"].FirstOrDefault();
                    var receiverRaw = form["ReceiverId"].FirstOrDefault() ?? form["receiverId"].FirstOrDefault();
                    if (!Guid.TryParse(senderRaw, out var senderId) || !Guid.TryParse(receiverRaw, out var receiverId))
                    {
                        return null;
                    }

                    var content = form["Content"].FirstOrDefault() ?? form["content"].FirstOrDefault();
                    var video = form.Files.GetFile("VideoUrl") ?? form.Files.GetFile("videoUrl");
                    var images = form.Files
                        .Where(f => string.Equals(f.Name, "ImageUrls", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    return new CreateMessageFormDTO
                    {
                        SenderId = senderId,
                        ReceiverId = receiverId,
                        Content = content,
                        VideoUrl = video,
                        ImageUrls = images.Count > 0 ? images : null
                    };
                }

                using var jsonDoc = await JsonDocument.ParseAsync(Request.Body);
                var root = jsonDoc.RootElement;

                string? senderRawJson = null;
                if (root.TryGetProperty("SenderId", out var senderProp) || root.TryGetProperty("senderId", out senderProp))
                {
                    senderRawJson = senderProp.GetString();
                }

                string? receiverRawJson = null;
                if (root.TryGetProperty("ReceiverId", out var receiverProp) || root.TryGetProperty("receiverId", out receiverProp))
                {
                    receiverRawJson = receiverProp.GetString();
                }

                if (!Guid.TryParse(senderRawJson, out var senderIdJson) || !Guid.TryParse(receiverRawJson, out var receiverIdJson))
                {
                    return null;
                }

                string? contentJson = null;
                if (root.TryGetProperty("Content", out var contentProp) || root.TryGetProperty("content", out contentProp))
                {
                    contentJson = contentProp.ValueKind == JsonValueKind.String ? contentProp.GetString() : null;
                }

                return new CreateMessageFormDTO
                {
                    SenderId = senderIdJson,
                    ReceiverId = receiverIdJson,
                    Content = contentJson,
                    VideoUrl = null,
                    ImageUrls = null
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
