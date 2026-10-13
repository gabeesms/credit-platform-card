using CardService.Domain.Entities;

namespace CardService.Application.Interfaces;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Card>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    Task<Card?> ChangeStatusAsync(Guid id, CardStatus status, CancellationToken cancellationToken);
    Task<bool> ProcessApprovedEventAsync(Guid eventId, Card card, CancellationToken cancellationToken);
}
