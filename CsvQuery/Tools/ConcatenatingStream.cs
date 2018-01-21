using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvQuery.Tools
{
    public class ConcatenatingStream : Stream
    {
        private readonly bool _closeStreams;
        private Stream _current;
        private IEnumerator<Stream> _iterator;
        private long _position;

        public ConcatenatingStream(IEnumerable<Stream> source, bool closeStreams)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _iterator = source.GetEnumerator();
            _closeStreams = closeStreams;
        }

        public static ConcatenatingStream FromFactoryFunctions<T>(bool closeStreams, params Func<T>[] factories)
            where T : Stream
        {
            return new ConcatenatingStream(factories.Select(factory => factory()), closeStreams);
        }

        private Stream NextStream()
        {
            if (_closeStreams)
                _current?.Dispose();

            _current = null;
            if (_iterator == null) throw new ObjectDisposedException(GetType().Name);
            if (_iterator.MoveNext()) _current = _iterator.Current;
            return _current;
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        private void EndOfStream()
        {
            if (_closeStreams)
                _current?.Dispose();

            _current = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_closeStreams)
                    do _iterator.Current?.Dispose(); while (_iterator.MoveNext());

                _iterator.Dispose();
                _iterator = null;
                _current = null;
            }

            base.Dispose(disposing);
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void WriteByte(byte value) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void Flush() { } // nothing to do
        

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = 0;
            while (count > 0)
            {
                var stream = _current ?? NextStream();
                if (stream == null) break;
                var thisCount = stream.Read(buffer, offset, count);
                result += thisCount;
                count -= thisCount;
                offset += thisCount;
                if (thisCount == 0) EndOfStream();
            }

            _position += result;
            return result;
        }
    }
}