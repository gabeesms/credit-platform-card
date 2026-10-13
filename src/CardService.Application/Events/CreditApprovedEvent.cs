namespace CardService.Application.Events;

public sealed record CreditApprovedEvent(
    Guid EventId,
    string EventType,
    int Version,
    DateTimeOffset OccurredAt,
    string? CorrelationId,
    Guid CreditApplicationId,
    Guid CustomerId,
    decimal Amount,
    string Status);
