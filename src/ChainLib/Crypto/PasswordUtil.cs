namespace ChainLib.Crypto
{
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Sodium;
    using System;
    using System.Text;

    public static class PasswordUtil
    {
        /// <summary>
        /// Produces a password hash suitable for long term storage. This means using a random salt per password, high entropy, and
        /// high number of key stretching operations.
        ///
        /// It's important to distinguish this from a Wallet address' private key.
        /// Normally, unless you're creating a "brain wallet", this should never be used as the seed for a private key, since
        /// remembering the password is the only thing necessary to derive a private key.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string StorageHash(string password, string salt = null)
        {
            byte[] saltBytes = ArgonSalt(salt);
            byte[] hashBytes = ArgonHash(password, saltBytes);
            return $"{Convert.ToBase64String(saltBytes)}:{Convert.ToBase64String(hashBytes)}";
        }

        /// <summary>
        /// Produces a password hash with a fixed salt in reasonably fast time.
        /// Used when we need a quasi-secure hash that is backed by something else
        /// more secure (i.e.: brain wallets where we use the argon hash as the password)
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] FastHash(string password, string salt) => KeyDerivation.Pbkdf2(password, Encoding.UTF8.GetBytes(salt), KeyDerivationPrf.HMACSHA512, 64000, 32);

        public static bool Verify(string password, string passwordHash)
        {
            string[] tokens = passwordHash.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] saltBytes = Convert.FromBase64String(tokens[0]);
            byte[] compareHashBytes = Convert.FromBase64String(tokens[1]);
            byte[] hashBytes = ArgonHash(password, saltBytes);
            return compareHashBytes.ConstantTimeEquals(hashBytes);
        }

        private static byte[] ArgonSalt(string salt) => salt == null ? PasswordHash.ArgonGenerateSalt() : Encoding.UTF8.GetBytes(salt);

        private static byte[] ArgonHash(string password, byte[] saltBytes)
        {
            byte[] hashBytes = PasswordHash.ArgonHashBinary(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                PasswordHash.StrengthArgon.Sensitive, 512);
            return hashBytes;
        }
    }
}