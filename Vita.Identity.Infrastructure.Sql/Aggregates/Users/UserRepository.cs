using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vita.Core.Domain.Repositories;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Infrastructure.Sql.Aggregates.Users;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    public IUnitOfWork UnitOfWork => _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<User> Add(User user)
    {
        var entry = _context.Users.Add(user);
        return Task.FromResult(entry.Entity);
    }

    public Task<User> FindByIdAsync(Guid id)
    {
        return _context.Users.Include(x => x.LoginProviders)
                             .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<User> FindByEmailAsync(Email email)
    {
        return _context.Users.Include(x => x.LoginProviders)
                             .FirstOrDefaultAsync(x => x.Email == email);
    }

    public Task<User> FindByLoginProvider(string loginProvider, string userId)
    {
        return _context.Users.Include(x => x.LoginProviders)
                             .FirstOrDefaultAsync(u => u.LoginProviders.Any(elp => elp.Name == loginProvider && elp.ExternalUserId == userId));
    }

    public Task Update(User user)
    {
        _context.Users.Attach(user);
        _context.Entry(user).State = EntityState.Modified;

        return Task.CompletedTask;
    }
}
