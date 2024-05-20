namespace ChainLib.Exceptions
{
    using System;

    public class BlockchainAssertionException : Exception
    {
        public BlockchainAssertionException(string message) : base(message) { }

        public BlockchainAssertionException(string message, Exception inner) : base(message, inner) { }
    }
}