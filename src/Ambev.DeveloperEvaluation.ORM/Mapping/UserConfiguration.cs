using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.OwnsOne(u => u.Username, username =>
        {
            username.Property(x => x.Value).HasColumnName("Username").IsRequired().HasMaxLength(50);
            username.HasIndex(x => x.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(x => x.Value).HasColumnName("Email").IsRequired().HasMaxLength(100);
            email.HasIndex(x => x.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Password, password =>
            password.Property(x => x.Value).HasColumnName("Password").IsRequired().HasMaxLength(100));

        builder.OwnsOne(u => u.Phone, phone =>
            phone.Property(x => x.Value).HasColumnName("Phone").IsRequired().HasMaxLength(20));

        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(100);
            name.Property(n => n.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(100);
        });

        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(a => a.City).HasColumnName("City").IsRequired().HasMaxLength(100);
            address.Property(a => a.Street).HasColumnName("Street").IsRequired().HasMaxLength(200);
            address.Property(a => a.Number).HasColumnName("Number").IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("ZipCode").IsRequired().HasMaxLength(20);

            address.OwnsOne(a => a.Geolocation, geolocation =>
            {
                geolocation.Property(g => g.Lat).HasColumnName("Lat").IsRequired().HasMaxLength(50);
                geolocation.Property(g => g.Long).HasColumnName("Long").IsRequired().HasMaxLength(50);
            });

            address.Navigation(a => a.Geolocation).IsRequired();
        });

        builder.Navigation(u => u.Username).IsRequired();
        builder.Navigation(u => u.Email).IsRequired();
        builder.Navigation(u => u.Password).IsRequired();
        builder.Navigation(u => u.Phone).IsRequired();
        builder.Navigation(u => u.Name).IsRequired();
        builder.Navigation(u => u.Address).IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
