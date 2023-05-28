using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Vita.Core.Infrastructure.Sql;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Infrastructure.Sql.Aggregates.Users;

public class UserEntityConfiguration : EntityTypeConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .ValueGeneratedNever()
               .IsRequired();

        builder.Property(u => u.Email)
               .IsRequired()
               .HasConversion(v => v.Address, v => new Email(v));

        builder.Property(u => u.GivenName)
               .IsRequired();

        builder.Property(u => u.FamilyName)
               .IsRequired();

        builder.HasIndex(u => u.Email)
               .IsUnique();

        builder.HasMany(u => u.LoginProviders)
               .WithOne()
               .HasForeignKey("UserId")
               .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(User.LoginProviders));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
