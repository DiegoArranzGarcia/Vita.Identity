using AutoFixture;
using Vita.Identity.Domain.Tests.AutoFixture;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Tests.Aggregates.Users
{
    public static class UserTestsFixture
    {
        private readonly static IFixture _fixture = new Fixture().Customize(new EmailCustomization());

        public readonly static Email GoodEmail = _fixture.Create<Email>();
        public readonly static string GivenName = _fixture.Create<string>();
        public readonly static string FamilyName = _fixture.Create<string>();
        public readonly static string PasswordHash = _fixture.Create<string>();
    }
}
