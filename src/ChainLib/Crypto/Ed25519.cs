namespace ChainLib.Crypto
{
    using Sodium;
    using System;
    using System.Text;

    public class Ed25519
    {
        public static Tuple<byte[], byte[]> GenerateKeyPairFromSecret(byte[] privateKeySeed)
        {
            Sodium.KeyPair keyPair = PublicKeyAuth.GenerateKeyPair(privateKeySeed);

            return new Tuple<byte[], byte[]>(keyPair.PublicKey, keyPair.PrivateKey);
        }

        public static Tuple<byte[], byte[]> GenerateKeyPairFromPrivateKey(byte[] privateKey)
        {
            byte[] publicKey = PublicKeyAuth.ExtractEd25519PublicKeyFromEd25519SecretKey(privateKey);

            return new Tuple<byte[], byte[]>(publicKey, privateKey);
        }

        public static byte[] Sign(Tuple<byte[], byte[]> keyPair, string message) => PublicKeyAuth.Sign(Encoding.UTF8.GetBytes(message), keyPair.Item2);

        public static bool VerifySignature(byte[] publicKey, byte[] signature, byte[] message) => PublicKeyAuth.VerifyDetached(signature, message, publicKey);
    }
}