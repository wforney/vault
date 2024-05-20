namespace ChainLib.Streaming
{
    using ChainLib.Models;
    using System.Collections.Generic;

    public class BlockObjectProjection
    {
        private readonly IBlockStore _source;
        private readonly IBlockObjectTypeProvider _typeProvider;

        public BlockObjectProjection(IBlockStore source, IBlockObjectTypeProvider typeProvider)
        {
            this._source = source;
            this._typeProvider = typeProvider;
        }

        public IEnumerable<T> Stream<T>(bool forwards = true, int startingAt = 0) where T : IBlockSerialized
        {
            long? type = this._typeProvider.Get(typeof(T));
            if (!type.HasValue)
            {
                yield break;
            }

            foreach (BlockObject item in this._source.StreamAllBlockObjects(forwards, startingAt))
            {
                if (!item.Type.HasValue || item.Type != type)
                {
                    continue;
                }

                yield return (T)item.Data;
            }
        }
    }
}
