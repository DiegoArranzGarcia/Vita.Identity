using MediatR;
using System;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Application.Commands.Users
{
    public record CreateUserCommand : IRequest<Guid>
    {
        public Email Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
