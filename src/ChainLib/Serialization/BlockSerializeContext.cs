namespace ChainLib.Serialization
{
    using ChainLib.Models;
    using System;
    using System.IO;

    public partial class BlockSerializeContext
    {
        public BlockSerializeContext(BinaryWriter bw, IBlockObjectTypeProvider typeProvider, int version = formatVersion)
        {
            this.bw = bw;
            this.typeProvider = typeProvider;
            if (this.Version > formatVersion)
            {
                throw new Exception("Tried to save block with a version that is too new");
            }

            this.Version = version;

            bw.Write(this.Version);
        }

        public readonly BinaryWriter bw;
        public readonly IBlockObjectTypeProvider typeProvider;

        #region Version

        public const int formatVersion = 1;

        public int Version { get; private set; }

        #endregion
    }
}