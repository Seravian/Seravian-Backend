using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 128-bit salt
    private const int KeySize = 256 / 8; // 256-bit hash
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derive the key
        byte[] key = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            Iterations,
            KeySize
        );

        // Combine salt + hash and encode
        var result = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(key, 0, result, SaltSize, KeySize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var decoded = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(decoded, 0, salt, 0, SaltSize);

        var keyToCheck = KeyDerivation.Pbkdf2(
            providedPassword,
            salt,
            KeyDerivationPrf.HMACSHA256,
            Iterations,
            KeySize
        );

        for (int i = 0; i < KeySize; i++)
        {
            if (decoded[SaltSize + i] != keyToCheck[i])
                return false;
        }

        return true;
    }
}
