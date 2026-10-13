using CardService.Application.Interfaces;
using CardService.Domain.Entities;

namespace CardService.Application.UseCases;

public sealed class ChangeCardStatusHandler(ICardRepository repository)
{
    public Task<Card?> HandleAsync(
        Guid id,
        CardStatus status,
        CancellationToken cancellationToken) =>
        repository.ChangeStatusAsync(id, status, cancellationToken);
}
