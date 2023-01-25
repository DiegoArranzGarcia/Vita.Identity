using Dawn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ICollection<LoginProvider> _loginProviders;
        public IEnumerable<LoginProvider> LoginProviders => _loginProviders;

        public static User CreateWithPassword(Email email, string givenName, string familyName, string passwordHash)
        {
            return new User(email, givenName, familyName, passwordHash);
        }

        public static User CreateWithLoginProvider(Email email, string givenName, string familyName, LoginProvider loginProvider = null)
        {
            return new User(email, givenName, familyName, loginProvider: loginProvider);
        }

        private User() { }

        private User(Email email, string givenName, string familyName, string passwordHash = null, LoginProvider loginProvider = null)
        {
            Id = Guid.NewGuid();
            Email = email;
            GivenName = Guard.Argument(givenName, nameof(givenName)).NotNull().NotEmpty();
            FamilyName = Guard.Argument(familyName, nameof(familyName)).NotNull().NotEmpty();

            Guard.NotAllNull(Guard.Argument(passwordHash), Guard.Argument(loginProvider));

            PasswordHash = passwordHash;
            _loginProviders = new List<LoginProvider>();

            if (loginProvider != null) _loginProviders.Add(loginProvider);
        }

        public void AssociateExternalLoginProvider(LoginProvider externalLoginProvider)
        {
            Guard.Argument(externalLoginProvider).NotNull();

            _loginProviders.Add(externalLoginProvider);
        }
    }
}