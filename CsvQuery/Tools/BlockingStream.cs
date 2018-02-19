namespace CsvQuery.Tools
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A read/write stream that blocks the writer when the reader gets too far behind
    /// </summary>
    /// <remarks>
    /// Usage example: <code>
    ///   using (var stream = new BlockingStream(10))
    ///   {
    ///       var producer = stream.StartProducer(s =&gt; WriteToStream(s));
    ///       var consumer = stream.StartConsumer(s =&gt; ReadFromStream(s));
    ///       Task.WaitAll(producer, consumer);
    ///   }
    /// </code>
    /// </remarks><inheritdoc />
    public class BlockingStream : Stream
    {
        private readonly BlockingCollection<byte[]> _blocks;
        private byte[] _currentBlock;
        private int _currentBlockIndex;

        /// <summary>
        /// Creates a read/write stream that blocks the writer when the reader gets too far behind
        /// </summary>
        /// <param name="streamWriteCountCache"> How many writes ahead the writer is allowed before blocking </param>
        public BlockingStream(int streamWriteCountCache)
        {
            this._blocks = new BlockingCollection<byte[]>(streamWriteCountCache);
        }

        /// <summary>
        /// Starts a task that writes to this stream
        /// </summary>
        public Task StartProducer(Action<BlockingStream> producingAction)
        {
            return Task.Factory.StartNew(s =>
            {
                var writeStream = (BlockingStream)s;
                producingAction(writeStream);
                writeStream.CompleteWriting();
            }, this);
        }

        /// <summary>
        /// Starts a task that reads from this file
        /// </summary>
        public Task StartConsumer(Action<BlockingStream> consumingAction)
        {
            return Task.Factory.StartNew(s =>
            {
                var readStream = (BlockingStream)s;
                consumingAction(readStream);
            }, this);
        }

        public override bool CanTimeout => false;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public long TotalBytesWritten { get; private set; }
        public int WriteCount { get; private set; }

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateBufferArgs(buffer, offset, count);

            var bytesRead = 0;
            while (true)
            {
                if (this._currentBlock != null)
                {
                    var copy = Math.Min(count - bytesRead, this._currentBlock.Length - this._currentBlockIndex);
                    Array.Copy(this._currentBlock, this._currentBlockIndex, buffer, offset + bytesRead, copy);
                    this._currentBlockIndex += copy;
                    bytesRead += copy;

                    if (this._currentBlock.Length <= this._currentBlockIndex)
                    {
                        this._currentBlock = null;
                        this._currentBlockIndex = 0;
                    }

                    if (bytesRead == count)
                        return bytesRead;
                }

                if (!this._blocks.TryTake(out this._currentBlock, Timeout.Infinite))
                    return bytesRead;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateBufferArgs(buffer, offset, count);

            var newBuf = new byte[count];
            Array.Copy(buffer, offset, newBuf, 0, count);
            this._blocks.Add(newBuf);
            this.TotalBytesWritten += count;
            this.WriteCount++;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                this._blocks.Dispose();
        }

        public override void Close()
        {
            this.CompleteWriting();
            base.Close();
        }

        /// <summary>
        /// Must be called when producer has finished writing data (unless StartProducer(...) is used)
        /// </summary>
        public void CompleteWriting() => this._blocks.CompleteAdding();

        private static void ValidateBufferArgs(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count)
                throw new ArgumentException("buffer.Length - offset < count");
        }
    }
}