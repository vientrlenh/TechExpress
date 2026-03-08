using System.ClientModel.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Service;
using TechExpress.Service.Constants;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ServiceProviders serviceProvider, UserContext userContext, IHubContext<ChatHub> chatHubContext, IConfiguration configuration) : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider = serviceProvider;
        private readonly UserContext _userContext = userContext;
        private readonly IHubContext<ChatHub> _chatHubContext = chatHubContext;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            var session = await _serviceProvider.ChatService.HandleCreateSession(userId, request.FullName?.Trim(), request.Phone?.Trim());
            var response = ResponseMapper.MapToChatSessionResponseFromChatSession(session);
            await _chatHubContext.Clients.Group("staff").SendAsync(SignalRMessageConstant.NewChatSession, response);
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
            var (newMsg, shouldTriggerAi) = await _serviceProvider.ChatService.HandleSendMessage(sessionId, userId, request.Phone?.Trim(), request.Message, mediaReq);
            var response = ResponseMapper.MapToChatMessageResponseFromChatMessage(newMsg);
            await _chatHubContext.Clients.Group($"chat-{sessionId}").SendAsync(SignalRMessageConstant.ChatMessageReceive, response);

            var aiApiKey = _configuration["AI:ApiKey"];
            if (shouldTriggerAi && aiApiKey is not null)
            {
                var aiMsg = await _serviceProvider.ChatService.HandleGenerateAiReply(sessionId);
                if (aiMsg is not null)
                {
                    var aiResponse = ResponseMapper.MapToChatMessageResponseFromChatMessage(aiMsg);
                    await _chatHubContext.Clients.Group($"chat-{sessionId}").SendAsync(SignalRMessageConstant.ChatMessageReceive, aiResponse);
                }
            }
            return Ok(ApiResponse<ChatMessageResponse>.OkResponse(response));
        }
    }
}
