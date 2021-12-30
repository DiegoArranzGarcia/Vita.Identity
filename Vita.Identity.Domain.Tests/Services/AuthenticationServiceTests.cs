using AutoFixture;
using System.Threading.Tasks;
using Vita.Identity.Domain.ValueObjects;
using Xunit;

namespace Vita.Identity.Domain.Tests.Services
{
    public class AuthenticationServiceTests
    {
        [Fact]
        public async Task WhenAuthenticatingUser_GivenRightUserAndPasswordHash_ShouldReturnTrue()
        {
            var providedPasssword = new Fixture().Create<string>();

            var user = AuthenticationServiceTestsFixture.CreateUser();

            var authenticationService = AuthenticationServiceTestsFixture.CreateAuthenticationServiceForUser(user, providedPasssword);

            Assert.True(await authenticationService.AuthenticateUser(user.Email, providedPasssword));
        }

        [Fact]
        public async Task WhenAuthenticatingUser_GivenRightUserButWrongPasswordHash_ShouldReturnFalse()
        {
            var providedPasssword = new Fixture().Create<string>();

            var user = AuthenticationServiceTestsFixture.CreateUser();

            var authenticationService = AuthenticationServiceTestsFixture.CreateAuthenticationServiceForUser(user, providedPasssword);

            Assert.False(await authenticationService.AuthenticateUser(user.Email, "wrong_password"));
        }

        [Fact]
        public async Task WhenAuthenticatingUser_GivenOtherUser_ShouldReturnFalse()
        {
            var providedPasssword = new Fixture().Create<string>();

            var user = AuthenticationServiceTestsFixture.CreateUser();
            var otherUser = AuthenticationServiceTestsFixture.CreateUser();

            var authenticationService = AuthenticationServiceTestsFixture.CreateAuthenticationServiceForUser(user, providedPasssword);

            Assert.False(await authenticationService.AuthenticateUser(otherUser.Email, providedPasssword));
        }

        [Fact]
        public async Task WhenAuthenticatingUser_GivenUnexistingEmail_ShouldReturnFalse()
        {
            var providedPasssword = new Fixture().Create<string>();
            var email = new Email("other_user_email@user.test.com");

            var user = AuthenticationServiceTestsFixture.CreateUser();
            var authenticationService = AuthenticationServiceTestsFixture.CreateAuthenticationServiceForUser(user, providedPasssword);

            Assert.False(await authenticationService.AuthenticateUser(email, providedPasssword));
        }
    }
}
