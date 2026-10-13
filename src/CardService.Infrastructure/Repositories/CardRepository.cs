using CardService.Application.Interfaces;
using CardService.Domain.Entities;
using CardService.Infrastructure.Messaging;
using CardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CardService.Infrastructure.Repositories;

public sealed class CardRepository(CardDbContext context) : ICardRepository
{
    public Task<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Cards.AsNoTracking().SingleOrDefaultAsync(card => card.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Card>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken) =>
        await context.Cards.AsNoTracking()
            .Where(card => card.CustomerId == customerId)
            .OrderByDescending(card => card.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Card?> ChangeStatusAsync(
        Guid id,
        CardStatus status,
        CancellationToken cancellationToken)
    {
        var card = await context.Cards.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);
        if (card is null)
        {
            return null;
        }

        card.ChangeStatus(status);
        await context.SaveChangesAsync(cancellationToken);
        return card;
    }

    public async Task<bool> ProcessApprovedEventAsync(
        Guid eventId,
        Card card,
        CancellationToken cancellationToken)
    {
        if (await context.ProcessedCreditEvents.AnyAsync(
                item => item.EventId == eventId,
                cancellationToken))
        {
            return false;
        }

        var existingCard = await context.Cards.AnyAsync(
            item => item.CreditApplicationId == card.CreditApplicationId,
            cancellationToken);

        context.ProcessedCreditEvents.Add(new ProcessedCreditEvent(eventId, DateTimeOffset.UtcNow));
        if (existingCard)
        {
            await context.SaveChangesAsync(cancellationToken);
            return false;
        }

        context.Cards.Add(card);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
