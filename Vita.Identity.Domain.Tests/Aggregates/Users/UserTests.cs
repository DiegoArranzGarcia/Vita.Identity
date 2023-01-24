using System;
using Vita.Identity.Domain.Aggregates.Users;
using Xunit;

namespace Vita.Identity.Domain.Tests.Aggregates.Users
{
    public class UserTests
    {
        [Fact]
        public void GivenValidArguments_WhenCreatingUser_ShouldCreateUser()
        {
            var user = User.CreateUserWithPassword(UserTestsFixture.GoodEmail,
                                                   UserTestsFixture.GivenName,
                                                   UserTestsFixture.FamilyName,
                                                   UserTestsFixture.PasswordHash);

            Assert.NotNull(user);
            Assert.Equal(expected: UserTestsFixture.GoodEmail, actual: user.Email);
            Assert.Equal(expected: UserTestsFixture.GivenName, actual: user.GivenName);
            Assert.Equal(expected: UserTestsFixture.FamilyName, actual: user.FamilyName);
            Assert.Equal(expected: UserTestsFixture.PasswordHash, actual: user.PasswordHash);
        }

        [Fact]
        public void WhenGettingFullName_ShouldReturnTheFullName()
        {
            var user = User.CreateUserWithPassword(UserTestsFixture.GoodEmail,
                                                   UserTestsFixture.GivenName,
                                                   UserTestsFixture.FamilyName,
                                                   UserTestsFixture.PasswordHash);

            Assert.Equal(expected: $"{UserTestsFixture.GivenName} {UserTestsFixture.FamilyName}".Trim(),
                           actual: user.FullName);
        }

        [Fact]
        public void WhenGettingUserName_ShouldReturnTheUserName()
        {
            var user = User.CreateUserWithPassword(UserTestsFixture.GoodEmail,
                                                   UserTestsFixture.GivenName,
                                                   UserTestsFixture.FamilyName,
                                                   UserTestsFixture.PasswordHash);

            Assert.Equal(expected: $"{UserTestsFixture.GivenName.ToLower()}.{UserTestsFixture.FamilyName.ToLower()}".Trim(),
                           actual: user.UserName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenBadGivenName_WhenCreatingUser_ShouldThrowException(string badGivenName)
        {
            Assert.ThrowsAny<ArgumentException>(() => User.CreateUserWithPassword(UserTestsFixture.GoodEmail,
                                                                                  badGivenName,
                                                                                  UserTestsFixture.FamilyName,
                                                                                  UserTestsFixture.PasswordHash));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenBadFamilyName_WhenCreatingUser_ShouldThrowException(string badFamilyName)
        {
            Assert.ThrowsAny<ArgumentException>(() => User.CreateUserWithPassword(UserTestsFixture.GoodEmail,
                                                                                  UserTestsFixture.GivenName,
                                                                                  badFamilyName,
                                                                                  UserTestsFixture.PasswordHash));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GivenBadPasswordHash_WhenCreatingUser_ShouldThrowException(string badPasswordHash)
        {
            Assert.ThrowsAny<ArgumentException>(() => User.CreateUserWithPassword(UserTestsFixture.GoodEmail,
                                                                                  UserTestsFixture.GivenName,
                                                                                  UserTestsFixture.FamilyName,
                                                                                  badPasswordHash));
        }
    }
}
