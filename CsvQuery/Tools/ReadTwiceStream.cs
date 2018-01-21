namespace CsvQuery.Tools
{
    using System;
    using System.IO;

    /// <summary>
    /// Read-only <see cref="Stream"/> that allows ONE rewind to start
    /// </summary>
    public class ReadTwiceStream : Stream
    {
        private readonly Stream _underlyingStream;
        private readonly bool _leaveOpen;
        private MemoryStream _cache;
        private int _phase;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlyingStream"> The stream to read from </param>
        public ReadTwiceStream(Stream underlyingStream) : this(underlyingStream, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlyingStream"> The stream to read from </param>
        /// <param name="leaveOpen"> <see langword="true" /> to leave the stream open after the <see cref="T:System.IO.StreamWriter" /> object is disposed; otherwise, <see langword="false" />.</param>
        public ReadTwiceStream(Stream underlyingStream, bool leaveOpen)
        {
            _underlyingStream = underlyingStream;
            _leaveOpen = leaveOpen;
            _cache = new MemoryStream();
            _phase = 1;
        }

        /// <inheritdoc cref="Stream.Flush"/>/>
        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_phase==1 && origin == SeekOrigin.Begin)
            {
                _phase = 2;
                return _cache.Seek(offset, SeekOrigin.Begin);
            }
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_phase == 1)
            {
                // Reading and caching it
                var read = _underlyingStream.Read(buffer, offset, count);
                _cache.Write(buffer, offset, read);
                return read;
            }

            if (_phase == 2)
            {
                // Replying the cached data
                var read = _cache.Read(buffer, offset, count);

                if (read == count)
                    return read;

                // Memorystream has reached it's end
                _phase = 3;

                // Read the rest from the real stream in phase 3
                offset += read;
                count -= read;

                // We don't need cache anymore
                this._cache.Dispose();
                this._cache = null;
            }

            // Phase 3: Just pipe underlying stream
            return _underlyingStream.Read(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _cache?.Dispose();
                _underlyingStream.Dispose();
            }
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => _phase == 2 ? _cache.Position : _underlyingStream.Position;
            set => this.Seek(value, SeekOrigin.Begin); 
        }
    }
}
