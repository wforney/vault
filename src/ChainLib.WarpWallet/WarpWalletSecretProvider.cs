namespace ChainLib.WarpWallet;

using ChainLib.Crypto;
using ChainLib.Wallets;
using ChainLib.WarpWallet.Internal;
using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Uses warpwallet's algorithm to produce a wallet secret:
/// <code>
///     s1 = scrypt(key=(passphrase||0x1), salt=(salt||0x1), N=2^18, r=8, p=1, dkLen=32)
///     s2 = pbkdf2(key=(passphrase||0x2), salt=(salt||0x2), c=2^16, dkLen=32, prf=HMAC_SHA256)
/// </code>
/// <see href="https://keybase.io/warp" />
/// </summary>
public class WarpWalletSecretProvider : IWalletSecretProvider
{
    private static readonly byte[] OneByte = { 0x1 };
    private static readonly byte[] TwoByte = { 0x2 };

    public byte[] GenerateSecret(params object[] args)
    {
        string passphrase = (string)args[0];
        string salt = (string)args[1];

        byte[] passphraseBytes = Encoding.UTF8.GetBytes(passphrase);
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

        byte[] scryptPassphrase = passphraseBytes.ConcatArrays(OneByte);
        byte[] scryptSalt = saltBytes.ConcatArrays(OneByte);
        byte[] pbkdfPassphase = passphraseBytes.ConcatArrays(TwoByte);
        byte[] pbkdfSalt = saltBytes.ConcatArrays(TwoByte);

        byte[] s1 = SCrypt.ComputeDerivedKey(scryptPassphrase, scryptSalt, (int)Math.Pow(2, 18), 8, 1, null, 32);
        byte[] s2 = Pbkdf2.ComputeDerivedKey(new HMACSHA256(pbkdfPassphase), pbkdfSalt, (int)Math.Pow(2, 16), 32);

        byte[] s3 = { };
        for (int i = 0; i < s1.Length; i++)
        {
            s3 = s3.ConcatArrays(new[] { (byte)(s1[i] ^ s2[i]) });
        }

        return s3;
    }
}