using System.ComponentModel.DataAnnotations;
using CardService.Domain.Entities;

namespace CardService.API.Models.Cards;

/// <summary>Requests a status change for an existing card.</summary>
public sealed record ChangeCardStatusRequest
{
    [EnumDataType(typeof(CardStatus))]
    public required CardStatus Status { get; init; }
}
