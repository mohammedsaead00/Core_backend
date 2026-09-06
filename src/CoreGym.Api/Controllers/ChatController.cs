using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly IMessagingService _messaging;

    public ChatController(CoreGymDbContext db, IMessagingService messaging, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
        _messaging = messaging;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> ListConversations(CancellationToken cancellationToken)
    {
        var conversations = await _db.Conversations
            .AsNoTracking()
            .Where(c => c.ClientId == UserId || c.CoachId == UserId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Take(50)
            .Select(c => new ChatConversationResponse(
                c.Id, c.ClientId, c.CoachId, c.SubscriptionId,
                c.LastMessage, c.LastMessageAt, c.ClientUnread, c.CoachUnread, c.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(conversations);
    }

    private async Task<Conversation> LoadParticipantConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var conversation = await _db.Conversations
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        if (conversation is null)
        {
            throw new KeyNotFoundException($"Conversation '{conversationId}' was not found.");
        }

        if (UserId != conversation.ClientId && UserId != conversation.CoachId)
        {
            throw new UnauthorizedAccessException("Only conversation participants can access messages.");
        }

        return conversation;
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> ListMessages(Guid id, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        await LoadParticipantConversationAsync(id, cancellationToken);
        limit = Math.Clamp(limit, 1, 200);
        var messages = await _db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == id)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(messages);
    }

    /// <summary>notify_new_message + update_conversation_on_message replacement (push integration is Phase 3).</summary>
    [HttpPost("conversations/{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await _messaging.SendMessageAsync(id, UserId, request.Content, request.Type, request.FileUrl, cancellationToken);
        return Created($"api/chat/conversations/{id}/messages", message);
    }

    /// <summary>mark_conversation_read replacement.</summary>
    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await LoadParticipantConversationAsync(id, cancellationToken);
        var marked = await _messaging.MarkConversationReadAsync(id, UserId, cancellationToken);
        return Ok(new MarkedReadResponse(marked));
    }

    /// <summary>unread_count replacement.</summary>
    [HttpGet("unread")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var count = await _messaging.GetUnreadCountAsync(UserId, cancellationToken);
        return Ok(new UnreadCountResponse(count));
    }
}
