namespace ChainLib.Models
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Value64
    {
        public uint v1, v2;

        public static bool operator ==(Value64 a, Value64 b) => a.v1 == b.v1 && a.v2 == b.v2;

        public static bool operator !=(Value64 a, Value64 b) => !(a == b);

        public byte[] ToBytes()
        {
            byte[] result = new byte[4 * 4];

            result[0] = (byte)(this.v1 >> (0 * 8));
            result[1] = (byte)(this.v1 >> (1 * 8));
            result[2] = (byte)(this.v1 >> (2 * 8));
            result[3] = (byte)(this.v1 >> (3 * 8));

            result[4] = (byte)(this.v2 >> (0 * 8));
            result[5] = (byte)(this.v2 >> (1 * 8));
            result[6] = (byte)(this.v2 >> (2 * 8));
            result[7] = (byte)(this.v2 >> (3 * 8));

            return result;
        }

        public override bool Equals(object obj) => obj is Value64 value64 && value64 == this;

        public override int GetHashCode() => (int)(this.v1 ^ this.v2);

        public override string ToString() => string.Format("{0:X8}{1:X8}", this.v1, this.v2);
    }
}