namespace ChainLib.Models
{
    using ChainLib.Serialization;

    public interface IBlockSerialized
    {
        void Serialize(BlockSerializeContext context);
    }
}