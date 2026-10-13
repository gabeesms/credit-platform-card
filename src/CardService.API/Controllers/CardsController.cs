using CardService.API.Models.Cards;
using CardService.Application.UseCases;
using CardService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CardService.API.Controllers;

[ApiController]
[Route("api")]
public sealed class CardsController(
    GetCardHandler getCardHandler,
    GetCustomerCardsHandler getCustomerCardsHandler,
    ChangeCardStatusHandler changeCardStatusHandler) : ControllerBase
{
    /// <summary>Returns a card by its internal identifier.</summary>
    [HttpGet("cards/{id:guid}")]
    [ProducesResponseType<CardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var card = await getCardHandler.HandleAsync(id, cancellationToken);
        return card is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Card not found.")
            : Ok(CardResponse.From(card));
    }

    /// <summary>Returns all cards registered for a customer.</summary>
    [HttpGet("customers/{customerId:guid}/cards")]
    [ProducesResponseType<IReadOnlyList<CardResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CardResponse>>> GetByCustomer(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var cards = await getCustomerCardsHandler.HandleAsync(customerId, cancellationToken);
        return Ok(cards.Select(CardResponse.From).ToArray());
    }

    /// <summary>Changes a card to active, blocked, or cancelled.</summary>
    [HttpPut("cards/{id:guid}/status")]
    [ProducesResponseType<CardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeCardStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var card = await changeCardStatusHandler.HandleAsync(
                id,
                request.Status,
                cancellationToken);
            return card is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Card not found.")
                : Ok(CardResponse.From(card));
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid card status transition.",
                detail: exception.Message);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid card status.",
                detail: exception.Message);
        }
    }
}
