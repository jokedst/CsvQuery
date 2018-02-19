namespace CsvQuery.Tools
{
    using System;
    using System.IO;

    /// <summary>
    /// A stream wrapper that only allows reading
    /// </summary>
    public class ReadStream : Stream
    {
        private readonly Stream _underlyingStream;
        public ReadStream(Stream underlyingStream)
        {
            if (!underlyingStream.CanRead) throw new ArgumentException("Can't read from underlying stream", nameof(underlyingStream));
            this._underlyingStream = underlyingStream;
        }
        public override void Flush() => this._underlyingStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => this._underlyingStream.Read(buffer, offset, count);
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Length => this._underlyingStream.Length;
        public override long Position
        {
            get => this._underlyingStream.Position;
            set => throw new NotSupportedException();
        }
    }
    
    /// <summary>
    /// A stream wrapper that only allows writing
    /// </summary>
    public class WriteStream : Stream
    {
        private readonly Stream _underlyingStream;

        public WriteStream(Stream underlyingStream)
        {
            if(!underlyingStream.CanWrite) throw new ArgumentException("Can't write to underlying stream", nameof(underlyingStream));
            this._underlyingStream = underlyingStream;
        }

        public override void Flush() => this._underlyingStream.Flush();
        public override void Write(byte[] buffer, int offset, int count) => this._underlyingStream.Write(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => this._underlyingStream.Length;
        public override long Position
        {
            get => this._underlyingStream.Position;
            set => this._underlyingStream.Position = value;
        }
    }
}