using Ambev.DeveloperEvaluation.Application.Common.Lists;
using Ambev.DeveloperEvaluation.Application.Common.Results;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListCategories;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Application.Products.ListProductsByCategory;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Products.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IMediator mediator) : ControllerBase
{
    private const string CatalogWriters = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}";

    [HttpGet]
    [ProducesResponseType(typeof(ListProductsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProducts(CancellationToken cancellationToken)
    {
        var page = await mediator.Send(new ListProductsQuery(ParseList()), cancellationToken);

        return Ok(ListProductsResponse.From(page));
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCategories(CancellationToken cancellationToken)
    {
        var categories = await mediator.Send(new ListCategoriesQuery(), cancellationToken);

        return Ok(categories);
    }

    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ListProductsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProductsByCategory(
        [FromRoute] string category,
        CancellationToken cancellationToken)
    {
        var page = await mediator.Send(new ListProductsByCategoryQuery(category, ParseList()), cancellationToken);

        return Ok(ListProductsResponse.From(page));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ProductResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpPost]
    [Authorize(Roles = CatalogWriters)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCreateCommand(), cancellationToken);

        if (result.IsFailure)
            return Problem(result.Error!);

        var response = ProductResponse.From(result.Value);

        return CreatedAtAction(nameof(GetProduct), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = CatalogWriters)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] ProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToUpdateCommand(id), cancellationToken);

        return result.IsSuccess
            ? Ok(ProductResponse.From(result.Value))
            : Problem(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = CatalogWriters)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteProductCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : Problem(result.Error!);
    }

    private ListQuery ParseList() =>
        ListQueryParser.Parse(Request.Query.ToDictionary(
            entry => entry.Key,
            entry => (string?)entry.Value.ToString()));

    private IActionResult Problem(Error error) =>
        StatusCode(ApiResults.StatusFor(error.Category), ApiResults.ToResponse(error));
}
