using CardService.Domain.Entities;
using CardService.Infrastructure.Persistence;
using CardService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CardService.Tests.Infrastructure;

public sealed class CardRepositoryTests
{
    [Fact]
    public async Task ProcessApprovedEventAsync_DuplicateDeliveryDoesNotCreateSecondCard()
    {
        var options = new DbContextOptionsBuilder<CardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var eventId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        await using (var context = new CardDbContext(options))
        {
            var repository = new CardRepository(context);
            var card = Card.Issue(applicationId, customerId, 5_000m);
            Assert.True(await repository.ProcessApprovedEventAsync(
                eventId,
                card,
                CancellationToken.None));
        }

        await using (var context = new CardDbContext(options))
        {
            var repository = new CardRepository(context);
            var duplicate = Card.Issue(applicationId, customerId, 5_000m);
            Assert.False(await repository.ProcessApprovedEventAsync(
                eventId,
                duplicate,
                CancellationToken.None));

            var duplicateApproval = Card.Issue(applicationId, customerId, 5_000m);
            Assert.False(await repository.ProcessApprovedEventAsync(
                Guid.NewGuid(),
                duplicateApproval,
                CancellationToken.None));
            Assert.Single(await context.Cards.ToListAsync());
            Assert.Equal(2, await context.ProcessedCreditEvents.CountAsync());
        }
    }
}
