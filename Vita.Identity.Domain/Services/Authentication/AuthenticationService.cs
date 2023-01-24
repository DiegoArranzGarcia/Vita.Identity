using System.Threading.Tasks;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.Services.Passwords;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _usersRepository;
        private readonly IPasswordService _passwordService;

        public AuthenticationService(IUserRepository usersRepository, IPasswordService passwordService)
        {
            _usersRepository = usersRepository;
            _passwordService = passwordService;
        }

        public async Task<bool> AuthenticateUser(Email email, string password)
        {
            var user = await _usersRepository.FindByEmailAsync(email);

            if (user == null)
                return false;

            return _passwordService.VerifyHashedPassword(hashedPassword: user.PasswordHash, providedPassword: password);
        }
    }
}
