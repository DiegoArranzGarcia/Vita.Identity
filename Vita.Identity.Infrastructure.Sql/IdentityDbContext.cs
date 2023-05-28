﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Vita.Core.Infrastructure.Sql;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Infrastructure.Sql.Aggregates.Users;

namespace Vita.Identity.Infrastructure.Sql;

public class IdentityDbContext : VitaDbContext
{
    private readonly IMediator _mediator;

    public DbSet<User> Users { get; private set; }
    public DbSet<LoginProvider> LoginProviders { get; private set; }

    public IdentityDbContext(DbContextOptions options, IMediator mediator) : base(options, mediator)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new LoginProviderEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
