using System.Threading.Tasks;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Services.Authentication;

public interface IAuthenticationService
{
    Task<bool> AuthenticateUser(Email email, string password);
}
