namespace ChainLib.Models
{
    using ChainLib.Serialization;
    using System;

    public interface IBlockObjectTypeProvider
    {
        byte[] SecretKey { get; }
        bool TryAdd(long id, Type type);
        long? Get(Type type);
        Type Get(long typeId);
        IBlockSerialized Deserialize(Type type, BlockDeserializeContext context);
    }
}