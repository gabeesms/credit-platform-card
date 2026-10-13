using CardService.Application.Events;
using CardService.Application.Interfaces;
using CardService.Domain.Entities;

namespace CardService.Application.UseCases;

public sealed class CreditApprovedEventHandler(ICardRepository repository)
{
    public async Task<bool> HandleAsync(
        CreditApprovedEvent message,
        CancellationToken cancellationToken)
    {
        if (message.EventId == Guid.Empty
            || message.CreditApplicationId == Guid.Empty
            || message.CustomerId == Guid.Empty
            || message.EventType != "CreditApproved"
            || message.Version != 1
            || message.Status != "Approved"
            || message.Amount <= 0)
        {
            throw new InvalidDataException("CreditApproved event has an unsupported or invalid payload.");
        }

        var card = Card.Issue(message.CreditApplicationId, message.CustomerId, message.Amount);
        return await repository.ProcessApprovedEventAsync(message.EventId, card, cancellationToken);
    }
}
