using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.ORM.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public const string ItemsNavigation = "_items";

    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(s => s.Number)
            .HasConversion(SaleValueConverters.SaleNumberConverter)
            .HasColumnName("sale_number")
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.SoldAt).HasColumnName("sold_at").IsRequired();
        builder.Property(s => s.CartId).HasColumnName("cart_id").IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Total)
            .HasConversion(SaleValueConverters.MoneyConverter)
            .HasColumnName("total")
            .HasColumnType(SaleValueConverters.AmountColumnType)
            .IsRequired();

        builder.Property(s => s.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.ModifiedAt).HasColumnName("modified_at");
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        builder.OwnsOne(s => s.Customer, customer =>
        {
            customer.Property(c => c.Id).HasColumnName("customer_id").IsRequired();
            customer.Property(c => c.Name).HasColumnName("customer_name").IsRequired().HasMaxLength(200);
            customer.HasIndex(c => c.Id);
        });

        builder.OwnsOne(s => s.Branch, branch =>
        {
            branch.Property(b => b.Id).HasColumnName("branch_id").IsRequired();
            branch.Property(b => b.Name).HasColumnName("branch_name").IsRequired().HasMaxLength(200);
            branch.HasIndex(b => b.Id);
        });

        builder.Navigation(s => s.Customer).IsRequired();
        builder.Navigation(s => s.Branch).IsRequired();

        builder.HasMany<SaleItem>(ItemsNavigation)
            .WithOne()
            .HasForeignKey("SaleId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(ItemsNavigation).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(s => s.Items);

        builder.HasIndex(s => s.Number).IsUnique();
        builder.HasIndex(s => s.SoldAt);
        builder.HasIndex(s => s.Status);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
