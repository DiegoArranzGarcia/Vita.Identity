using Dawn;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Vita.Core.Domain;

namespace Vita.Identity.Domain.ValueObjects;

public class Email : ValueObject
{
    private const string EmailPattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
    private const RegexOptions EmailRegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase;
    private readonly static Regex EmailRegex = new(EmailPattern, EmailRegexOptions);

    public string Address { get; init; }

    public Email(string address)
    {
        Address = Guard.Argument(address, nameof(address)).NotEmpty()
                                                          .NotNull()
                                                          .Matches(EmailRegex).Value.ToLower();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Address;
    }
}
