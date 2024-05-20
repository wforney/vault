namespace ChainLib.Models
{
    using ChainLib.Serialization;

    public struct BlockHeader : IBlockSerialized, IBlockDescriptor
    {
        public int Version { get; set; }
        public byte[] PreviousHash { get; set; }
        public byte[] MerkleRootHash { get; set; }
        public uint Timestamp { get; set; }
        public uint Difficulty { get; set; }
        public long Nonce { get; set; }

        public void DeserializeHeader(BlockDeserializeContext context)
        {
            this.Version = context.br.ReadInt32();           // Version
            this.PreviousHash = context.br.ReadBuffer();     // PreviousHash
            this.MerkleRootHash = context.br.ReadBuffer();   // MerkleRootHash
            this.Timestamp = context.br.ReadUInt32();        // Timestamp
            this.Difficulty = context.br.ReadUInt32();       // Difficulty
            this.Nonce = context.br.ReadInt64();             // Nonce
        }

        public static void Serialize(IBlockDescriptor descriptor, BlockSerializeContext context)
        {
            context.bw.Write(context.Version);                     // Version
            context.bw.WriteBuffer(descriptor.PreviousHash);       // PreviousHash
            context.bw.WriteBuffer(descriptor.MerkleRootHash);     // MerkleRootHash
            context.bw.Write(descriptor.Timestamp);                // Timestamp
            context.bw.Write(descriptor.Difficulty);               // Difficulty
            context.bw.Write(descriptor.Nonce);                    // Nonce
        }

        public void Serialize(BlockSerializeContext context) => Serialize(this, context);

        public static void Deserialize(IBlockDescriptor descriptor, BlockDeserializeContext context)
        {
            descriptor.Version = context.br.ReadInt32();           // Version
            descriptor.PreviousHash = context.br.ReadBuffer();     // PreviousHash
            descriptor.MerkleRootHash = context.br.ReadBuffer();   // MerkleRootHash
            descriptor.Timestamp = context.br.ReadUInt32();        // Timestamp
            descriptor.Difficulty = context.br.ReadUInt32();       // Difficulty
            descriptor.Nonce = context.br.ReadInt64();             // Nonce
        }
    }
}