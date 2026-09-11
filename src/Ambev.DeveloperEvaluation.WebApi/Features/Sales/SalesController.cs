using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ListSalesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSales(CancellationToken cancellationToken)
    {
        var page = await mediator.Send(new ListSalesQuery(ParseList(), User.ToCallerContext()), cancellationToken);

        return Ok(ListSalesResponse.From(page));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSaleQuery(id, User.ToCallerContext()), cancellationToken);

        return result.IsSuccess
            ? Ok(SaleResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateSale(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCommand(User.ToCallerContext()), cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error!);

        var response = SaleResponse.From(result.Value);

        return CreatedAtAction(nameof(GetSale), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSale(
        [FromRoute] Guid id,
        [FromBody] UpdateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCommand(id, User.ToCallerContext()), cancellationToken);

        return result.IsSuccess
            ? Ok(SaleResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale(
        [FromRoute] Guid id,
        [FromBody] CancelSaleRequest? request,
        CancellationToken cancellationToken)
    {
        var command = (request ?? new CancelSaleRequest()).ToCommand(id, User.ToCallerContext());
        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(SaleResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSaleCommand(id, User.ToCallerContext()), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error!);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}/cancel")]
    [ProducesResponseType(typeof(SaleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSaleItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CancelSaleItemCommand(id, itemId, User.ToCallerContext()),
            cancellationToken);

        return result.IsSuccess
            ? Ok(SaleResponse.From(result.Value))
            : Problem(result.Error!);
    }

    private ListQuery ParseList() =>
        ListQueryParser.Parse(Request.Query.ToDictionary(
            entry => entry.Key,
            entry => (string?)entry.Value.ToString()));

    private IActionResult Problem(Error error) =>
        StatusCode(ApiResults.StatusFor(error.Category), ApiResults.ToResponse(error));
}
