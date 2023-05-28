using System.Security.Cryptography;

namespace Vita.Identity.Domain.Services.Passwords;

public class PasswordServiceOptions
{
    private static readonly RandomNumberGenerator _defaultRng = RandomNumberGenerator.Create();
    public RandomNumberGenerator Rng { get; set; } = _defaultRng;
    public int IterationCount { get; set; } = 10000;
}
