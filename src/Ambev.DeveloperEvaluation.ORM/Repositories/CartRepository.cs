using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class CartRepository(DefaultContext context) : ICartRepository
{
    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await context.Carts.AddAsync(cart, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Carts.FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);
    }

    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        context.Carts.Update(cart);
        await context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cart = await GetByIdAsync(id, cancellationToken);

        if (cart is null)
            return false;

        context.Carts.Remove(cart);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
