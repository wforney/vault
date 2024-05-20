namespace ChainLib.Streaming
{
    using ChainLib.Models;
    using reactive.pipes.Producers;

    public class BlockObjectProducer<T> : BackgroundProducer<T> where T : IBlockSerialized
    {
        public BlockObjectProducer(BlockObjectProjection objectProjection) => this.Background.Produce(objectProjection.Stream<T>());
    }
}