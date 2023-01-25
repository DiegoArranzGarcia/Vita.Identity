using Dawn;
using System;
using Vita.Core.Domain.Repositories;

namespace Vita.Identity.Domain.Aggregates.Users
{
    public class LoginProvider : Entity
    {
        public string Name { get; init; }
        public string ExternalUserId { get; init; }
        public string Token { get; private set; }

        private LoginProvider() { }

        public LoginProvider(string name, string externalUserId, string token)
        {
            Id = Guid.NewGuid();
            Name = Guard.Argument(name).NotNull().NotEmpty();
            ExternalUserId = Guard.Argument(externalUserId).NotNull().NotEmpty();
            Token = Guard.Argument(token).NotNull().NotEmpty();
        }

        public void UpdateAccessToken(string token)
        {
            Guard.Argument(token).NotNull().NotEmpty();

            Token = token;
        }
    }
}
