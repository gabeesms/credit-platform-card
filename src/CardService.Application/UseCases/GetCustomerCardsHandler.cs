using CardService.Application.Interfaces;
using CardService.Domain.Entities;

namespace CardService.Application.UseCases;

public sealed class GetCustomerCardsHandler(ICardRepository repository)
{
    public Task<IReadOnlyList<Card>> HandleAsync(Guid customerId, CancellationToken cancellationToken) =>
        repository.GetByCustomerIdAsync(customerId, cancellationToken);
}
