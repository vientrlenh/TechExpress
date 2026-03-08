using System;
using System.Diagnostics.Eventing.Reader;
using PayOS.Exceptions;
using TechExpress.Repository;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Services;

public class ChatService
{
    private readonly UnitOfWork _unitOfWork;

    public ChatService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatSession> HandleCreateSession(string? userIdStr, string? fullName, string? phone)
    {
        ChatSession? unclosedSession;
        Guid? userId;
        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng: {userId}");
            unclosedSession = await _unitOfWork.ChatSessionRepository.FindByUserIdAndNotClosedAsync(userId.Value);
            fullName = user.FirstName + user.LastName;
            phone = null;
        }
        else if (fullName is not null && phone is not null)
        {
            unclosedSession = await _unitOfWork.ChatSessionRepository.FindByPhoneAndNotClosedAsync(phone);
            userId = null;
        }
        else
        {
            throw new BadRequestException("Tên đầy đủ và số điện thoại là bắt buộc để thực hiện tạo phiên hội thoại");
        }
        if (unclosedSession is null)
        {
            unclosedSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = fullName,
                Phone = phone
            };
            await _unitOfWork.ChatSessionRepository.AddAsync(unclosedSession);
            await _unitOfWork.SaveChangesAsync();
        }

        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(unclosedSession.Id) ?? throw new NotFoundException($"Không tìm thấy phiên trò chuyện {unclosedSession.Id}");
        return session;
    }

    public async Task<(List<ChatMessage>, int, bool)> HandleLoadMessages(Guid sessionId, string? userIdStr, string? phone, int size, int pageIndex)
    {
        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(sessionId) ?? throw new NotFoundException($"Không tìm thấy phiên trò chuyện {sessionId}");
        Guid? userId;
        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng {userId}");
            if (user.IsCustomerUser() && session.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện này");
            }
        }
        else if (phone is not null)
        {
            if (session.Phone != phone)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện");
            }
        }
        else
        {
            throw new BadRequestException("Đăng nhập hoặc yêu cầu thông qua số điện thoại để truy cập vào cuộc trò chuyện");
        }
        var messages = await _unitOfWork.ChatMessageRepository.FindChatMessagesIncludeMediasBySessionIdWithSplitQueryAsync(sessionId, size, pageIndex);
        bool isMore = messages.Count > size;
        if (isMore)
        {
            messages.RemoveAt(messages.Count - 1);
        }
        messages.Reverse();
        return (messages, pageIndex, isMore);
    }

    public async Task<ChatMessage> HandleSendMessage(Guid sessionId, string? userIdStr, string? phone, string message, List<(string, ChatMediaType)> medias)
    {
        var session = await _unitOfWork.ChatSessionRepository.FindByIdAsync(sessionId) ?? throw new NotFoundException($"Không tìm thấy phiên trò chuyện {sessionId}");
        Guid? userId;
        string? sentByFullName;
        if (userIdStr is not null)
        {
            userId = Guid.Parse(userIdStr);
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId.Value) ?? throw new NotFoundException($"Không tìm thấy người dùng {userId}");
            if (user.IsCustomerUser() && session.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện này");
            }
            sentByFullName = user.IsCustomerUser() ? user.FirstName + user.LastName : "Nhân viên hỗ trợ";
        }
        else if (phone is not null)
        {
            if (session.Phone != phone)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập vào phiên trò chuyện");
            }
            sentByFullName = session.FullName;
            userId = null;
        }
        else
        {
            throw new BadRequestException("Đăng nhập hoặc yêu cầu thông qua số điện thoại để truy cập vào cuộc trò chuyện");
        }
        if (medias.Count > 5)
        {
            throw new BadRequestException("Chỉ hỗ trợ tối đa 5 file phương tiện");
        }
        var messageId = Guid.NewGuid();
        var msg = new ChatMessage
        {
            Id = messageId,
            SessionId = sessionId,
            Message = message,
            SentById = userId,
            SentByFullName = sentByFullName,
            Medias = [.. medias.Select(m => new ChatMedia
            {
                MessageId = messageId,
                MediaUrl = m.Item1,
                Type = m.Item2
            })]
        };
        await _unitOfWork.ChatMessageRepository.AddAsync(msg);
        await _unitOfWork.SaveChangesAsync();

        var newMsg = await _unitOfWork.ChatMessageRepository.FindByIdIncludeMediasAsync(msg.Id) ?? throw new NotFoundException($"Không tìm thấy tin nhắn {msg.Id}");
        return newMsg;
    }
}
