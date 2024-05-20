namespace ChainLib.Serialization
{
    using ChainLib.Models;
    using System;
    using System.IO;

    public class BlockDeserializeContext
    {
        public BlockDeserializeContext(BinaryReader br, IBlockObjectTypeProvider typeProvider)
        {
            this.br = br;
            this.typeProvider = typeProvider;

            this.Version = br.ReadInt32();

            if (this.Version > BlockSerializeContext.formatVersion)
            {
                throw new Exception("Tried to load block with a version that is too new");
            }
        }

        public readonly BinaryReader br;
        public readonly IBlockObjectTypeProvider typeProvider;

        public int Version { get; }
    }
}