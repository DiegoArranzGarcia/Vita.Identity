using AutoFixture;
using NSubstitute;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.Services.Authentication;
using Vita.Identity.Domain.Services.Passwords;
using Vita.Identity.Domain.Tests.AutoFixture;
using Vita.Identity.Domain.ValueObjects;

namespace Vita.Identity.Domain.Tests.Services;

public static class AuthenticationServiceTestsFixture
{
    private readonly static IFixture _fixture = new Fixture().Customize(new EmailCustomization());

    public static Email GenerateEmail() => _fixture.Create<Email>();

    public static AuthenticationService CreateSut(IUserRepository? userRepository = null,
                                                  IPasswordService? passwordService = null)
    {
        return new AuthenticationService(userRepository ?? Substitute.For<IUserRepository>(),
                                         passwordService ?? Substitute.For<IPasswordService>());
    }

    public static AuthenticationService CreateAuthenticationServiceForUser(User user, string providedPassword)
    {
        var userRepository = CreateUserRepositoryWithUser(user);
        var passwordRepository = CreatePasswordService(user.PasswordHash, providedPassword);

        return CreateSut(userRepository, passwordRepository);
    }

    public static User CreateUser()
    {
        return _fixture.Build<User>()
                       .Without(x => x.Events)
                       .Create();
    }

    private static IUserRepository CreateUserRepositoryWithUser(User user)
    {
        var userRepository = Substitute.For<IUserRepository>();

        userRepository.FindByEmailAsync(Arg.Is(user.Email))
                      .Returns(user);

        return userRepository;
    }

    private static IPasswordService CreatePasswordService(string hashsedPassword, string providedPassword)
    {
        var passwordService = Substitute.For<IPasswordService>();

        passwordService.VerifyHashedPassword(Arg.Is(hashsedPassword), Arg.Is(providedPassword))
                       .Returns(true);

        return passwordService;
    }
}
