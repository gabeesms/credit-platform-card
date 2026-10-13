namespace CardService.Domain.Entities;

public sealed class Card
{
    private Card()
    {
    }

    private Card(Guid creditApplicationId, Guid customerId, decimal creditLimit, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        CreditApplicationId = creditApplicationId;
        CustomerId = customerId;
        CreditLimit = creditLimit;
        Type = CardType.StandardCredit;
        Status = CardStatus.Active;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid CreditApplicationId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal CreditLimit { get; private set; }
    public CardType Type { get; private set; }
    public CardStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Card Issue(Guid creditApplicationId, Guid customerId, decimal creditLimit)
    {
        if (creditApplicationId == Guid.Empty)
        {
            throw new ArgumentException("CreditApplicationId is required.", nameof(creditApplicationId));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId is required.", nameof(customerId));
        }

        if (creditLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(creditLimit), "Credit limit must be greater than zero.");
        }

        if (decimal.Round(creditLimit, 2) != creditLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(creditLimit), "Credit limit can have at most two decimal places.");
        }

        return new Card(creditApplicationId, customerId, creditLimit, DateTimeOffset.UtcNow);
    }

    public void ChangeStatus(CardStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Card status is invalid.");
        }

        if (Status == CardStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled card cannot change status.");
        }

        Status = status;
    }
}
