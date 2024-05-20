namespace ChainLib.Models
{
    using ChainLib.Serialization;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    public class BlockObjectTypeProvider : IBlockObjectTypeProvider
    {
        public byte[] SecretKey { get; }

        public BlockObjectTypeProvider(byte[] secretKey = null)
        {
            this.SecretKey = secretKey;
            this.Map = new ConcurrentDictionary<long, Type>();
            this.ReverseMap = new ConcurrentDictionary<Type, long>();
            this.Serializers = new ConcurrentDictionary<Type, ConstructorInfo>();
        }

        public IDictionary<long, Type> Map { get; }
        public IDictionary<Type, long> ReverseMap { get; }
        public IDictionary<Type, ConstructorInfo> Serializers { get; }

        public bool TryAdd(long id, Type type)
        {
            if (!this.Map.TryGetValue(id, out _))
            {
                this.Map.Add(id, type);
                this.ReverseMap.Add(type, id);
                this.Serializers.Add(type, type.GetConstructor(new[] { typeof(BlockDeserializeContext) }));
                return true;
            }

            return false;
        }

        public long? Get(Type type) => !this.ReverseMap.TryGetValue(type, out long result) ? (long?)null : result;

        public Type Get(long typeId) => !this.Map.TryGetValue(typeId, out Type result) ? null : result;

        public IBlockSerialized Deserialize(Type type, BlockDeserializeContext context)
        {
            if (!this.Serializers.TryGetValue(type, out ConstructorInfo serializer))
            {
                return null;
            }

            object deserialized = serializer.Invoke(new object[] { context });
            return (IBlockSerialized)deserialized;
        }
    }
}