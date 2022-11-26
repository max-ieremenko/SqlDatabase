using System;
using System.IO;

namespace SqlDatabase.IO;

internal partial class ZipFolderFile
{
    private sealed class EntryStream : Stream
    {
        private readonly IDisposable _owner;
        private readonly Stream _source;

        public EntryStream(IDisposable owner, Stream source)
        {
            _owner = owner;
            _source = source;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _source.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            _owner.Dispose();
            base.Close();
        }
    }
}