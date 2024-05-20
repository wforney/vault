namespace ChainLib.Serialization
{
    using System.IO;

    public static class BinaryReadWriteExtensions
    {
        public static bool WriteBoolean(this BinaryWriter bw, bool value)
        {
            bw.Write(value);
            return value; // Returning the written value allows for easy null checks
        }

        public static void WriteNullableString(this BinaryWriter bw, string value)
        {
            if (bw.WriteBoolean(value != null))
            {
                bw.Write(value);
            }
        }

        public static string ReadNullableString(this BinaryReader br) => br.ReadBoolean() ? br.ReadString() : null;

        public static void WriteNullableSingle(this BinaryWriter bw, float? value)
        {
            if (bw.WriteBoolean(value.HasValue))
            {
                bw.Write(value.Value);
            }
        }

        public static float? ReadNullableSingle(this BinaryReader br) => br.ReadBoolean() ? br.ReadSingle() : (float?)null;

        public static void WriteNullableLong(this BinaryWriter bw, long? value)
        {
            if (bw.WriteBoolean(value.HasValue))
            {
                bw.Write(value.Value);
            }
        }

        public static long? ReadNullableLong(this BinaryReader br) => br.ReadBoolean() ? br.ReadInt64() : (long?)null;

        public static void WriteBuffer(this BinaryWriter bw, byte[] buffer)
        {
            bool hasBuffer = buffer != null;
            if (bw.WriteBoolean(hasBuffer) && hasBuffer)
            {
                bw.Write(buffer.Length);
                bw.Write(buffer);
            }
        }

        public static byte[] ReadBuffer(this BinaryReader br)
        {
            if (!br.ReadBoolean())
            {
                return null;
            }

            int length = br.ReadInt32();
            byte[] buffer = br.ReadBytes(length);
            return buffer;
        }
    }
}