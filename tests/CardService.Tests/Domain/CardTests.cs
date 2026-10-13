using CardService.Domain.Entities;

namespace CardService.Tests.Domain;

public sealed class CardTests
{
    [Fact]
    public void Issue_UsesApprovedAmountAndStartsActive()
    {
        var card = Card.Issue(Guid.NewGuid(), Guid.NewGuid(), 5_000m);

        Assert.Equal(5_000m, card.CreditLimit);
        Assert.Equal(CardType.StandardCredit, card.Type);
        Assert.Equal(CardStatus.Active, card.Status);
    }

    [Fact]
    public void ChangeStatus_CancelledCardCannotBeReactivated()
    {
        var card = Card.Issue(Guid.NewGuid(), Guid.NewGuid(), 5_000m);
        card.ChangeStatus(CardStatus.Cancelled);

        Assert.Throws<InvalidOperationException>(() => card.ChangeStatus(CardStatus.Active));
    }

    [Fact]
    public void Issue_RejectsNonPositiveLimit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Card.Issue(Guid.NewGuid(), Guid.NewGuid(), 0m));
    }
}
