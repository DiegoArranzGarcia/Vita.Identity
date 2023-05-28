using System;
using Vita.Identity.Domain.ValueObjects;
using Xunit;

namespace Vita.Identity.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@test.com")]
    [InlineData("test.with.dots@test.com")]
    [InlineData("test1234@test.com")]
    [InlineData("test_with.1234@test.com")]
    [InlineData("test@extended.domain.com")]
    public void GivenGoodEmails_WhenCreatingEmail_ShouldCreateIt(string address)
    {
        var email = new Email(address);

        Assert.Equal(expected: email.Address, actual: address);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("badEmail")]
    [InlineData("badEmail@test")]
    [InlineData("badEmail.com")]
    public void GivenBadEmails_WhenCreatingEmail_ShouldThrowArgumentException(string badEmails)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Email(badEmails));
    }
}
