using System;
using System.Threading.Tasks;
using Vita.Core.Domain.Repositories;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Aggregates.Users
{
    public interface IUsersRepository : IRepository<User>
    {
        Task<User> FindByIdAsync(Guid id);
        Task<User> FindByEmailAsync(Email email);
        Task<User> Add(User user);
        Task<User> Update(User user);
    }
}
