namespace ChainLib.Exceptions
{
    using System;

    public class BlockAssertionException : Exception
    {
        public BlockAssertionException(string message) : base(message) { }

        public BlockAssertionException(string message, Exception inner) : base(message, inner) { }
    }
}