using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Vita.Core.Infrastructure.Sql;
using Vita.Identity.Domain.Aggregates.Users;

namespace Vita.Identity.Infrastructure.Sql.Aggregates.Users
{
    public class LoginProviderEntityConfiguration : EntityTypeConfiguration<LoginProvider>
    {
        public override void Configure(EntityTypeBuilder<LoginProvider> builder)
        {
            base.Configure(builder);

            builder.ToTable("LoginProviders");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .ValueGeneratedNever()
                   .IsRequired();

            builder.Property<Guid>("UserId")
                   .IsRequired();

            builder.Property(x => x.Name)
                   .IsRequired();

            builder.Property(x => x.ExternalUserId)
                   .IsRequired();

            builder.Property(x => x.Token)
                   .IsRequired();

            builder.HasIndex(x => new { x.Name, x.ExternalUserId })
                   .IsUnique();

            //builder.HasIndex(x => new[] { "UserId", nameof(LoginProvider.Name) })
            //       .IsUnique();
        }
    }
}
