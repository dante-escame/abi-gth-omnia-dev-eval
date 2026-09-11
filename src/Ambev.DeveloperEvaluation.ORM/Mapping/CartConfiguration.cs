using Ambev.DeveloperEvaluation.Domain.Carts;
using Ambev.DeveloperEvaluation.ORM.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(c => c.CustomerId).HasColumnName("user_id").IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.OwnsMany(c => c.Items, items =>
        {
            items.ToTable("cart_items");
            items.WithOwner().HasForeignKey("cart_id");
            items.Property<int>("Id").ValueGeneratedOnAdd();
            items.HasKey("Id");

            items.Property(i => i.Quantity)
                .HasConversion(CartValueConverters.QuantityConverter)
                .HasColumnName("quantity")
                .IsRequired();

            items.OwnsOne(i => i.Product, product =>
            {
                product.Property(p => p.Id).HasColumnName("product_id").IsRequired();
                product.Property(p => p.Title).HasColumnName("product_title").HasMaxLength(200);
                product.HasIndex(p => p.Id);
            });

            items.Navigation(i => i.Product).IsRequired();
        });

        builder.Navigation(c => c.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.Status);
    }
}
