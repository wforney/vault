namespace ChainLib.Models.Extended
{
    using ChainLib.Serialization;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class Transaction : IBlockSerialized
    {
        public enum TransactionType : byte { Unknown, Regular, Fee, Reward }

        public string Id { get; set; }
        public TransactionType Type { get; set; }
        public TransactionData Data { get; set; }

        public Transaction() { }

        public void Serialize(BlockSerializeContext context)
        {
            context.bw.WriteNullableString(this.Id);
            context.bw.Write((byte)this.Type);

            if (context.bw.WriteBoolean(this.Data != null))
            {
                Debug.Assert(this.Data != null);
                SerializeTransactionItems(context, this.Data.Inputs);
                SerializeTransactionItems(context, this.Data.Outputs);
            }
        }

        private static void SerializeTransactionItems(BlockSerializeContext context, ICollection<TransactionItem> items)
        {
            if (context.bw.WriteBoolean(items != null))
            {
                Debug.Assert(items != null);
                context.bw.Write(items.Count);
                foreach (TransactionItem input in items)
                {
                    context.bw.Write(input.Index);
                    context.bw.Write(input.TransactionId);
                    context.bw.Write((byte)input.Type);
                    context.bw.WriteBuffer(input.Address);
                    context.bw.Write(input.Amount);
                    context.bw.WriteBuffer(input.Signature);
                }
            }
        }

        public Transaction(BlockDeserializeContext context)
        {
            this.Id = context.br.ReadNullableString();
            this.Type = (TransactionType)context.br.ReadByte();

            if (context.br.ReadBoolean())
            {
                IList<TransactionItem> inputs = DeserializeTransactionItems(context);
                IList<TransactionItem> outputs = DeserializeTransactionItems(context);

                this.Data = new TransactionData
                {
                    Inputs = inputs,
                    Outputs = outputs
                };
            }
        }

        private static IList<TransactionItem> DeserializeTransactionItems(BlockDeserializeContext context)
        {
            List<TransactionItem> list = new List<TransactionItem>();

            if (context.br.ReadBoolean())
            {
                int count = context.br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    TransactionItem item = new TransactionItem
                    {
                        Index = context.br.ReadInt64(),
                        TransactionId = context.br.ReadString(),
                        Type = (TransactionDataType)context.br.ReadByte(),
                        Address = context.br.ReadBuffer(),
                        Amount = context.br.ReadInt64(),
                        Signature = context.br.ReadBuffer()
                    };

                    list.Add(item);
                }
            }

            return list;
        }
    }
}
