namespace CardService.Infrastructure.Messaging;

public sealed class ProcessedCreditEvent(Guid eventId, DateTimeOffset processedAt)
{
    public Guid EventId { get; private set; } = eventId;
    public DateTimeOffset ProcessedAt { get; private set; } = processedAt;
}
