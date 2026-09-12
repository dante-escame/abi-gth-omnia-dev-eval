using Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Carts.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListCartsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCarts(CancellationToken cancellationToken)
    {
        var page = await mediator.Send(new ListCartsQuery(ParseList(), User.ToCallerContext()), cancellationToken);

        return Ok(ListCartsResponse.From(page));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCartQuery(id, User.ToCallerContext()), cancellationToken);

        return result.IsSuccess
            ? Ok(CartResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCart(
        [FromBody] CartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCreateCommand(User.ToCallerContext()), cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error!);

        var response = CartResponse.From(result.Value);

        return CreatedAtAction(nameof(GetCart), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCart(
        [FromRoute] Guid id,
        [FromBody] CartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToUpdateCommand(id, User.ToCallerContext()), cancellationToken);

        return result.IsSuccess
            ? Ok(CartResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCart([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCartCommand(id, User.ToCallerContext()), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error!);
    }

    private ListQuery ParseList() =>
        ListQueryParser.Parse(Request.Query.ToDictionary(
            entry => entry.Key,
            entry => (string?)entry.Value.ToString()));

    private IActionResult Problem(Error error) =>
        StatusCode(ApiResults.StatusFor(error.Category), ApiResults.ToResponse(error));
}
