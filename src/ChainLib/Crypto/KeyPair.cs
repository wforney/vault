namespace ChainLib.Crypto
{
    using System;
    using System.Collections.Generic;

    public class KeyPair : IEquatable<KeyPair>
    {
        public int Index { get; }
        public byte[] PublicKey { get; }
        public byte[] PrivateKey { get; }

        internal KeyPair() { /* required for serialization */ }

        public KeyPair(int index, byte[] publicKey, byte[] privateKey)
        {
            this.Index = index;
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
        }

        public override bool Equals(object obj) => this.Equals(obj as KeyPair);

        public bool Equals(KeyPair other)
        {
            return other != null &&
                   this.Index == other.Index &&
                   EqualityComparer<byte[]>.Default.Equals(this.PublicKey, other.PublicKey) &&
                   EqualityComparer<byte[]>.Default.Equals(this.PrivateKey, other.PrivateKey);
        }

        public override int GetHashCode()
        {
            int hashCode = 605523053;
            hashCode = (hashCode * -1521134295) + this.Index.GetHashCode();
            hashCode = (hashCode * -1521134295) + EqualityComparer<byte[]>.Default.GetHashCode(this.PublicKey);
            hashCode = (hashCode * -1521134295) + EqualityComparer<byte[]>.Default.GetHashCode(this.PrivateKey);
            return hashCode;
        }

        public static bool operator ==(KeyPair pair1, KeyPair pair2) => EqualityComparer<KeyPair>.Default.Equals(pair1, pair2);

        public static bool operator !=(KeyPair pair1, KeyPair pair2) => !(pair1 == pair2);
    }
}