namespace PlayLeague.Api.Models;

public enum MessageType { MESSAGE, ANNOUNCEMENT }
public enum MessagePriority { LOW, NORMAL, HIGH, URGENT }
public enum DeliveryStatus { PENDING, SENT, FAILED }

public class LeagueMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Subject { get; set; }
    public required string Content { get; set; }
    public MessageType MessageType { get; set; } = MessageType.MESSAGE;
    public MessagePriority Priority { get; set; } = MessagePriority.NORMAL;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid LeagueId { get; set; }
    public League League { get; set; } = null!;

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public MessageTargeting? Targeting { get; set; }
    public ICollection<MessageRecipient> Recipients { get; set; } = [];
}

public class MessageTargeting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool EntireLeague { get; set; } = false;

    public Guid MessageId { get; set; }
    public LeagueMessage Message { get; set; } = null!;

    public Guid? DivisionId { get; set; }
    public Division? Division { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
}

public class MessageRecipient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime? SentAt { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.PENDING;

    public Guid MessageId { get; set; }
    public LeagueMessage Message { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
