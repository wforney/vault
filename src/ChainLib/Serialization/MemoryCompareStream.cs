namespace ChainLib.Serialization
{
    using System;
    using System.Diagnostics;
    using System.IO;

    internal class MemoryCompareStream : Stream
    {
        public MemoryCompareStream(byte[] compareTo)
        {
            this.compareTo = compareTo;
            this.position = 0;
        }

        private readonly byte[] compareTo;
        private long position;

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[offset + i] != this.compareTo[this.position + i])
                {

                    Debug.Assert(false);
                    throw new Exception("Data mismatch");
                }
            }

            this.position += count;
        }

        public override void WriteByte(byte value)
        {
            if (this.compareTo[this.position] != value)
            {
                Debug.Assert(false);
                throw new Exception("Data mismatch");
            }

            this.position++;
        }

        #region Boring Stream Stuff

        public override bool CanRead => false;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override void Flush() { }
        public override long Length => this.compareTo.Length;
        public override long Position { get => this.position; set => this.position = value; }

        public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.position = offset;
                    break;
                case SeekOrigin.Current:
                    this.position += offset;
                    break;
                case SeekOrigin.End:
                    this.position = this.compareTo.Length - offset;
                    break;
            }

            return this.Position;
        }

        public override void SetLength(long value) => throw new InvalidOperationException();

        #endregion

    }
}