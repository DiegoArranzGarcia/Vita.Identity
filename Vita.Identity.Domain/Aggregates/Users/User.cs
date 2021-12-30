using Dawn;
using System;
using Vita.Core.Domain.Repositories;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Aggregates.Users
{
    public class User : Entity
    {
        public string PasswordHash { get; init; }
        public Email Email { get; init; }
        public string GivenName { get; init; }
        public string FamilyName { get; init; }
        public string FullName => $"{GivenName} {FamilyName}".Trim();
        public string UserName => $"{GivenName.ToLower()}.{FamilyName.ToLower()}".Trim();

        protected User()
        {

        }

        public User(Email email, string givenName, string familyName, string passwordHash) : this()
        {
            Id = Guid.NewGuid();
            Email = email;
            PasswordHash = Guard.Argument(passwordHash, nameof(passwordHash)).NotNull().NotEmpty();
            GivenName = Guard.Argument(givenName, nameof(givenName)).NotNull().NotEmpty();
            FamilyName = Guard.Argument(familyName, nameof(familyName)).NotNull().NotEmpty();
        }
    }
}