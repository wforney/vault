namespace ChainLib.Models
{
    using ChainLib.Serialization;

    public class BlockObject : IBlockSerialized
    {
        public BlockObject() { }

        public long? Type { get; set; }
        public int Index { get; set; }
        public long Version { get; set; }
        public IBlockSerialized Data { get; set; }
        public long Timestamp { get; set; }

        [Computed]
        public byte[] Hash { get; set; }

        #region Serialization

        public object Deserialize(BlockDeserializeContext context) => new BlockObject(context);

        public void Serialize(BlockSerializeContext context)
        {
            this.Type = context.typeProvider.Get(this.Data?.GetType());

            context.bw.WriteNullableLong(this.Type);         // Type
            context.bw.Write(this.Version);               // Version
            context.bw.Write(this.Timestamp);             // Timestamp
            context.bw.WriteBuffer(this.Hash);            // Hash

            if (context.bw.WriteBoolean(this.Data != null) && this.Type.HasValue)
            {
                this.Data?.Serialize(context);
            }
        }

        public BlockObject(BlockDeserializeContext context)
        {
            this.Type = context.br.ReadNullableLong();      // Type
            this.Version = context.br.ReadInt64();          // Version
            this.Timestamp = context.br.ReadInt64();            // Timestamp
            this.Hash = context.br.ReadBuffer();                // Hash

            if (context.br.ReadBoolean() && this.Type.HasValue)
            {
                System.Type type = context.typeProvider.Get(this.Type.Value);
                if (type != null)
                {
                    this.Data = context.typeProvider.Deserialize(type, context);
                }
            }
        }

        #endregion
    }
}