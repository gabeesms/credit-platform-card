using CardService.Application.Events;
using CardService.Application.Interfaces;
using CardService.Application.UseCases;
using CardService.Domain.Entities;

namespace CardService.Tests.Application;

public sealed class CreditApprovedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_StoresCardOnlyFromValidApprovalEvent()
    {
        var repository = new FakeCardRepository();
        var handler = new CreditApprovedEventHandler(repository);
        var message = NewEvent();

        var created = await handler.HandleAsync(message, CancellationToken.None);

        Assert.True(created);
        Assert.Equal(message.CreditApplicationId, repository.Card?.CreditApplicationId);
        Assert.Equal(message.Amount, repository.Card?.CreditLimit);
        Assert.Equal(CardStatus.Active, repository.Card?.Status);
        Assert.Equal(message.EventId, repository.EventId);
    }

    [Fact]
    public async Task HandleAsync_RejectsRejectedCreditEvent()
    {
        var handler = new CreditApprovedEventHandler(new FakeCardRepository());

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            handler.HandleAsync(NewEvent() with { Status = "Rejected" }, CancellationToken.None));
    }

    private static CreditApprovedEvent NewEvent() =>
        new(
            Guid.NewGuid(),
            "CreditApproved",
            1,
            DateTimeOffset.UtcNow,
            "trace-id",
            Guid.NewGuid(),
            Guid.NewGuid(),
            5_000m,
            "Approved");

    private sealed class FakeCardRepository : ICardRepository
    {
        public Card? Card { get; private set; }
        public Guid EventId { get; private set; }

        public Task<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<Card?>(null);

        public Task<IReadOnlyList<Card>> GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Card>>([]);

        public Task<Card?> ChangeStatusAsync(
            Guid id,
            CardStatus status,
            CancellationToken cancellationToken) =>
            Task.FromResult<Card?>(null);

        public Task<bool> ProcessApprovedEventAsync(
            Guid eventId,
            Card card,
            CancellationToken cancellationToken)
        {
            EventId = eventId;
            Card = card;
            return Task.FromResult(true);
        }
    }
}
