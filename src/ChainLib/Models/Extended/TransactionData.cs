namespace ChainLib.Models.Extended
{
    using System.Collections.Generic;

    public class TransactionData
    {
        public IList<TransactionItem> Inputs { get; set; }
        public IList<TransactionItem> Outputs { get; set; }
    }
}