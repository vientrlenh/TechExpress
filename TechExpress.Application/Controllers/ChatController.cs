using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Service;
using TechExpress.Service.Contexts;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ServiceProviders serviceProvider, UserContext userContext) : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider = serviceProvider;
        private readonly UserContext _userContext = userContext;

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            var session = await _serviceProvider.ChatService.HandleCreateSession(userId, request.FullName?.Trim(), request.Phone?.Trim());
            var response = ResponseMapper.MapToChatSessionResponseFromChatSession(session);
            return CreatedAtAction(nameof(CreateSession), ApiResponse<ChatSessionResponse>.CreatedResponse(response));
        }

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> LoadMessages([FromRoute] Guid sessionId, [FromQuery] string? phone, [FromQuery] int size, [FromQuery] int pageIndex)
        {
            if (size < 0 || size > 20) size = 20;
            if (pageIndex < 1) pageIndex = 1;
            var userId = _userContext.GetCurrentAuthenticatedUserIdIfExist();

            var (messages, currentPage, isMore) = await _serviceProvider.ChatService.HandleLoadMessages(sessionId, userId, phone, size, pageIndex);
            var response = ResponseMapper.MapToChatMessageResponseList(messages, currentPage, isMore);
            return Ok(ApiResponse<ChatMessageResponseList>.OkResponse(response));
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<IActionResult> SendMessage([FromRoute] Guid sessionId, [FromBody] SendChatMessageRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            List<(string, ChatMediaType)> mediaReq = [.. request.Medias.Select(m => (m.MediaUrl, m.Type))];
            var newMsg = await _serviceProvider.ChatService.HandleSendMessage(sessionId, userId, request.Phone?.Trim(), request.Message, mediaReq);
            var response = ResponseMapper.MapToChatMessageResponseFromChatMessage(newMsg);
            return Ok(ApiResponse<ChatMessageResponse>.OkResponse(response));
        }
    }
}
