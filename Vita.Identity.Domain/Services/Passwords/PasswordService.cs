using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;

namespace Vita.Identity.Domain.Services.Passwords;

public class PasswordService : IPasswordService
{
    private readonly int _iterCount;
    private readonly RandomNumberGenerator _rng;

    public PasswordService(IOptions<PasswordServiceOptions> optionsAccessor = null)
    {
        var options = optionsAccessor?.Value ?? new PasswordServiceOptions();

        if (options.IterationCount < 1)
            throw new InvalidOperationException(Resources.InvalidPasswordHasherIterationCount);

        _iterCount = options.IterationCount;
        _rng = options.Rng;
    }

    public virtual string HashPassword(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));

        return Convert.ToBase64String(HashPasswordV3(password, _rng));
    }

    private byte[] HashPasswordV3(string password, RandomNumberGenerator rng)
    {
        return HashPasswordV3(password, rng,
            prf: KeyDerivationPrf.HMACSHA256,
            iterCount: _iterCount,
            saltSize: 128 / 8,
            numBytesRequested: 256 / 8);
    }

    private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
    {
        byte[] salt = new byte[saltSize];
        rng.GetBytes(salt);
        byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subkey.Length];
        outputBytes[0] = 0x01;
        WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
        WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
        return outputBytes;
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return (uint)buffer[offset + 0] << 24
            | (uint)buffer[offset + 1] << 16
            | (uint)buffer[offset + 2] << 8
            | buffer[offset + 3];
    }

    public virtual bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        if (hashedPassword == null)
            throw new ArgumentNullException(nameof(hashedPassword));

        if (providedPassword == null)
            throw new ArgumentNullException(nameof(providedPassword));

        byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

        if (decodedHashedPassword.Length == 0)
            return false;

        switch (decodedHashedPassword[0])
        {
            case 0x01:
                int embeddedIterCount;
                return VerifyHashedPasswordV3(decodedHashedPassword, providedPassword, out embeddedIterCount);

            default:
                return false;
        }
    }

    private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterCount)
    {
        iterCount = default;

        try
        {
            var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
            iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
            int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

            if (saltLength < 128 / 8)
                return false;
            byte[] salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

            int subkeyLength = hashedPassword.Length - 13 - salt.Length;
            if (subkeyLength < 128 / 8)
                return false;
            byte[] expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            return false;
        }
    }

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }
}
