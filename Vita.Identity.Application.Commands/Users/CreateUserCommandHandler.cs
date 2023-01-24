using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.Services.Passwords;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Application.Commands.Users
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IPasswordService _passwordService;

        public CreateUserCommandHandler(IUserRepository usersRepository, IPasswordService passwordService)
        {
            _usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }

        public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var user = User.CreateUserWithPassword(command.Email, command.FirstName, command.LastName, _passwordService.HashPassword(command.Password));

            await _usersRepository.Add(user);
            await _usersRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return user.Id;
        }
    }
}
