using CardService.Domain.Entities;

namespace CardService.API.Models.Cards;

/// <summary>Represents a simulated card managed by Card Service.</summary>
public sealed record CardResponse(
    Guid Id,
    Guid CreditApplicationId,
    Guid CustomerId,
    decimal CreditLimit,
    CardType Type,
    CardStatus Status,
    DateTimeOffset CreatedAt)
{
    public static CardResponse From(Card card) =>
        new(
            card.Id,
            card.CreditApplicationId,
            card.CustomerId,
            card.CreditLimit,
            card.Type,
            card.Status,
            card.CreatedAt);
}
