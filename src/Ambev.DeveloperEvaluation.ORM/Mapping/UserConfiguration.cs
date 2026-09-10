using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Users.ValueObjects;
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

        builder.Property(u => u.Username)
            .HasConversion(username => username.Value, value => new Username(value))
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Password)
            .HasConversion(password => password.Value, value => new PasswordHash(value))
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .HasConversion(email => email.Value, value => new Email(value))
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasConversion(phone => phone.Value, value => new Phone(value))
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.FirstName).HasColumnName("FirstName").IsRequired().HasMaxLength(100);
            name.Property(n => n.LastName).HasColumnName("LastName").IsRequired().HasMaxLength(100);
        });

        builder.Navigation(u => u.Name).IsRequired();

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

        builder.Navigation(u => u.Address).IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
