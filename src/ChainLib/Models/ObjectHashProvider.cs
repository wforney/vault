namespace ChainLib.Models
{
    using ChainLib.Crypto;
    using FastMember;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Buffers;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    public class ObjectHashProvider : IHashProvider
    {
        private readonly HashAlgorithm _algorithm;
        private readonly JsonSerializerSettings _settings;

        public ObjectHashProvider() : this(SHA256.Create()) { }

        public ObjectHashProvider(HashAlgorithm algorithm)
        {
            this._algorithm = algorithm;
            this._settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new StableHashResolver()
            };
        }

        public byte[] ComputeHashBytes(object instance)
        {
            if (instance is string s)
            {
                return this.ComputeHashBytes(s);
            }

            string json = JsonConvert.SerializeObject(instance, this._settings);
            return this.ComputeHashBytes(json);
        }

        public byte[] ComputeHashBytes(string any)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(any);
            byte[] hash = this._algorithm.ComputeHash(buffer);
            return hash;
        }

        public byte[] ComputeHashBytes(byte[] buffer) => this._algorithm.ComputeHash(buffer);

        public byte[] ComputeHashBytes(Stream stream) => this._algorithm.ComputeHash(stream);

        public string ComputeHashString(object instance) => this.ComputeHashBytes(instance).ToHex();

        public string ComputeHashString(string any) => this.ComputeHashBytes(any).ToHex();

        public string ComputeHashString(byte[] any) => this.ComputeHashBytes(any).ToHex();

        public byte[] DoubleHash(object any) => this.ComputeHashBytes(this.ComputeHashBytes(any));

        public byte[] DoubleHash(byte[] any) => this.ComputeHashBytes(this.ComputeHashBytes(any));

        public byte[] DoubleHash(byte[] one, byte[] two)
        {
            byte[] three = ArrayPool<byte>.Shared.Rent(one.Length + two.Length);
            Buffer.BlockCopy(one, 0, three, 0, one.Length);
            Buffer.BlockCopy(two, 0, three, three.Length, two.Length);

            byte[] result = this.ComputeHashBytes(this.ComputeHashBytes(three));
            ArrayPool<byte>.Shared.Return(three);
            return result;
        }

        public byte[] DoubleHash(string any) => this.ComputeHashBytes(this.ComputeHashBytes(any));

        private static readonly IDictionary<Type, IList<JsonProperty>> Map =
            new Dictionary<Type, IList<JsonProperty>>();

        private class StableHashResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                // treat computed properties as if they weren't there
                if (Attribute.IsDefined(member, typeof(ComputedAttribute)))
                {
                    property.ShouldSerialize = instance => false;
                }

                // treat null/empty collection properties as if they weren't there
                else if (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    property.ShouldSerialize = instance => IsEmptyEnumerable(member, instance);
                }

                return property;
            }

            private static bool IsEmptyEnumerable(MemberInfo member, object instance)
            {
                TypeAccessor accessor = TypeAccessor.Create(instance.GetType());
                object property = accessor[instance, member.Name];
                return !(property is IEnumerable enumerable) || enumerable.GetEnumerator().MoveNext();
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                lock (Map)
                {
                    if (!Map.TryGetValue(type, out IList<JsonProperty> properties))
                    {
                        Map.Add(type, properties = base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList());
                    }

                    return properties;
                }
            }
        }
    }
}