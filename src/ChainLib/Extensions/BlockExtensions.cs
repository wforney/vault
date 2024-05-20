namespace ChainLib.Extensions
{
    using ChainLib.Models;
    using System.Collections.Generic;
    using System.Linq;

    public static class BlockExtensions
    {
        public static string ToHashString(this Block block, IHashProvider hashProvider) => hashProvider.ComputeHashString(block);

        public static byte[] ToHashBytes(this Block block, IHashProvider hashProvider)
        {
            foreach (BlockObject @object in block?.Objects ?? Enumerable.Empty<BlockObject>())
            {
                @object.Hash = hashProvider.ComputeHashBytes(@object);
            }

            return hashProvider.ComputeHashBytes(block);
        }

        public static byte[] ToHashBytes(this IBlockSerialized data, IHashProvider hashProvider) => hashProvider.ComputeHashBytes(data);

        public static byte[] ComputeMerkleRoot(this Block block, IHashProvider hashProvider)
        {
            if (block.Objects == null || block.Objects.Count == 0)
            {
                return hashProvider.ComputeHashBytes(Block.NoObjects);
            }

            // https://en.bitcoin.it/wiki/Protocol_documentation#Merkle_Trees

            List<byte[]> p = new List<byte[]>();
            foreach (BlockObject o in block.Objects)
            {
                p.Add(hashProvider.DoubleHash(o));
            }

            if (p.Count > 1 && p.Count % 2 != 0)
            {
                p.Add(p[p.Count - 1]);
            }

            if (p.Count == 1)
            {
                return p[0];
            }

        pass:
            {
                List<byte[]> n = new List<byte[]>(p.Count / 2);
                for (int i = 0; i < p.Count; i++)
                {
                    for (int j = i + 1; j < p.Count; j++)
                    {
                        byte[] a = block.Objects[i].Hash;
                        byte[] b = block.Objects[j].Hash;
                        byte[] d = hashProvider.DoubleHash(a, b);
                        n.Add(d);
                        i++;
                    }
                }

                if (n.Count == 1)
                {
                    return n[0];
                }

                if (n.Count % 2 != 0)
                {
                    n.Add(n[n.Count - 1]);
                }

                p = n;
                goto pass;
            }
        }
    }
}