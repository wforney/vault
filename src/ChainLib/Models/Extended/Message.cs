namespace ChainLib.Models.Extended
{
    using ChainLib.Serialization;

    public class Message : IBlockSerialized
    {
        public string Text { get; set; }

        public Message(string text) => this.Text = text;

        public void Serialize(BlockSerializeContext context) => context.bw.Write(this.Text);

        public Message(BlockDeserializeContext context) => this.Text = context.br.ReadString();
    }
}