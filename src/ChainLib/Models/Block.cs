namespace ChainLib.Models
{
    using ChainLib.Serialization;
    using Sodium;
    using System.Collections.Generic;
    using System.IO;

    public class Block : IBlockDescriptor
    {
        internal static IReadOnlyCollection<BlockObject> NoObjects = new List<BlockObject>(0);

        public Block()
        {
            this.Version = 1;
            this.Objects = new List<BlockObject>();
        }

        public long? Index { get; set; }
        public int Version { get; set; }
        public byte[] PreviousHash { get; set; }
        public byte[] MerkleRootHash { get; set; }
        public uint Timestamp { get; set; }
        public uint Difficulty { get; set; }
        public long Nonce { get; set; }

        public IList<BlockObject> Objects { get; set; }

        [Computed]
        public byte[] Hash { get; set; }

        #region Serialization

        public void Serialize(BlockSerializeContext context)
        {
            this.SerializeHeader(context);
            context.bw.WriteBuffer(this.Hash);
            this.SerializeObjects(context);
        }

        private Block(BlockDeserializeContext context)
        {
            this.DeserializeHeader(context);
            this.Hash = context.br.ReadBuffer();
            this.DeserializeObjects(context);
        }

        public void DeserializeHeader(BlockDeserializeContext context) => BlockHeader.Deserialize(this, context);

        public void DeserializeObjects(BlockDeserializeContext context)
        {
            int count = context.br.ReadInt32();
            this.Objects = new List<BlockObject>(count);
            for (int i = 0; i < count; i++)
            {
                this.Objects.Add(new BlockObject(context));
            }
        }

        public void SerializeHeader(BlockSerializeContext context) => BlockHeader.Serialize(this, context);

        public void SerializeObjects(BlockSerializeContext context)
        {
            int count = this.Objects?.Count ?? 0;
            context.bw.Write(count);
            if (this.Objects != null)
            {
                foreach (BlockObject @object in this.Objects)
                {
                    @object.Serialize(context);
                }
            }
        }

        public void RoundTripCheck(IHashProvider hashProvider, IBlockObjectTypeProvider typeProvider)
        {
            // Serialize a first time
            MemoryStream firstMemoryStream = new MemoryStream();
            BlockSerializeContext firstSerializeContext = new BlockSerializeContext(new BinaryWriter(firstMemoryStream), typeProvider);

            byte[] nonce;
            if (typeProvider.SecretKey != null)
            {
                nonce = StreamEncryption.GenerateNonceChaCha20();
                firstSerializeContext.bw.WriteBuffer(nonce);
                using (MemoryStream ems = new MemoryStream())
                {
                    using (BinaryWriter ebw = new BinaryWriter(ems))
                    {
                        BlockSerializeContext ec = new BlockSerializeContext(ebw, typeProvider, firstSerializeContext.Version);
                        this.Serialize(ec);
                        firstSerializeContext.bw.WriteBuffer(StreamEncryption.EncryptChaCha20(ems.ToArray(), nonce, ec.typeProvider.SecretKey));
                    }
                }
            }
            else
            {
                firstSerializeContext.bw.Write(false);
                this.Serialize(firstSerializeContext);
            }

            byte[] originalData = firstMemoryStream.ToArray();

            // Then deserialize that data
            {
                BinaryReader br = new BinaryReader(new MemoryStream(originalData));
                BlockDeserializeContext deserializeContext = new BlockDeserializeContext(br, typeProvider);
                nonce = deserializeContext.br.ReadBuffer();

                Block deserialized;
                if (nonce != null)
                {
                    using (MemoryStream dms = new MemoryStream(StreamEncryption.DecryptChaCha20(deserializeContext.br.ReadBuffer(), nonce, typeProvider.SecretKey)))
                    {
                        using (BinaryReader dbr = new BinaryReader(dms))
                        {
                            BlockDeserializeContext dc = new BlockDeserializeContext(dbr, typeProvider);
                            deserialized = new Block(dc);
                        }
                    }
                }
                else
                {
                    deserialized = new Block(deserializeContext);
                }

                // Then serialize that deserialized data and see if it matches
                {
                    MemoryCompareStream secondMemoryStream = new MemoryCompareStream(originalData);
                    BlockSerializeContext secondSerializeContext = new BlockSerializeContext(new BinaryWriter(secondMemoryStream), typeProvider);
                    if (typeProvider.SecretKey != null)
                    {
                        secondSerializeContext.bw.WriteBuffer(nonce);
                        using (MemoryStream ems = new MemoryStream())
                        {
                            using (BinaryWriter ebw = new BinaryWriter(ems))
                            {
                                BlockSerializeContext ec = new BlockSerializeContext(ebw, typeProvider, secondSerializeContext.Version);
                                deserialized.Serialize(ec);
                                secondSerializeContext.bw.WriteBuffer(StreamEncryption.EncryptChaCha20(ems.ToArray(), nonce, ec.typeProvider.SecretKey));
                            }
                        }
                    }
                    else
                    {
                        secondSerializeContext.bw.Write(false);
                        deserialized.Serialize(secondSerializeContext);
                    }
                }
            }
        }

        #endregion
    }
}
