using Ambev.DeveloperEvaluation.Domain.Sales;
using Ambev.DeveloperEvaluation.ORM.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnType("uuid").ValueGeneratedNever();

        builder.Property<Guid>("SaleId").HasColumnName("sale_id");

        builder.Property(i => i.Quantity)
            .HasConversion(SaleValueConverters.QuantityConverter)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasConversion(SaleValueConverters.MoneyConverter)
            .HasColumnName("unit_price")
            .HasColumnType(SaleValueConverters.AmountColumnType)
            .IsRequired();

        builder.Property(i => i.Rate)
            .HasConversion(SaleValueConverters.DiscountRateConverter)
            .HasColumnName("discount_rate")
            .HasColumnType("numeric(4,2)")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(i => i.Product, product =>
        {
            product.Property(p => p.Id).HasColumnName("product_id").IsRequired();
            product.Property(p => p.Title).HasColumnName("product_title").IsRequired().HasMaxLength(200);
            product.HasIndex(p => p.Id);
        });

        builder.OwnsOne(i => i.Totals, totals =>
        {
            totals.Property(t => t.Gross)
                .HasConversion(SaleValueConverters.MoneyConverter)
                .HasColumnName("gross_amount")
                .HasColumnType(SaleValueConverters.AmountColumnType)
                .IsRequired();

            totals.Property(t => t.Discount)
                .HasConversion(SaleValueConverters.MoneyConverter)
                .HasColumnName("discount_amount")
                .HasColumnType(SaleValueConverters.AmountColumnType)
                .IsRequired();

            totals.Property(t => t.Net)
                .HasConversion(SaleValueConverters.MoneyConverter)
                .HasColumnName("net_amount")
                .HasColumnType(SaleValueConverters.AmountColumnType)
                .IsRequired();
        });

        builder.Navigation(i => i.Product).IsRequired();
        builder.Navigation(i => i.Totals).IsRequired();
    }
}
