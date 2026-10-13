using CardService.Application.Interfaces;
using CardService.Domain.Entities;

namespace CardService.Application.UseCases;

public sealed class GetCardHandler(ICardRepository repository)
{
    public Task<Card?> HandleAsync(Guid id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);
}
