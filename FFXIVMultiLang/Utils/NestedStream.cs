using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFXIVMultiLang.Utils;

// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See https://github.com/dotnet/Nerdbank.Streams/blob/00fded/LICENSE for full license information.

// Original: https://github.com/dotnet/Nerdbank.Streams/blob/00fded/src/Nerdbank.Streams/NestedStream.cs

/// <summary>
/// A stream that allows for reading from another stream up to a given number of bytes.
/// </summary>
public class NestedStream : Stream
{
    /// <summary>
    /// The stream to read from.
    /// </summary>
    private readonly Stream underlyingStream;

    /// <summary>
    /// The total length of the stream.
    /// </summary>
    private readonly long length;

    /// <summary>
    /// The remaining bytes allowed to be read.
    /// </summary>
    private long remainingBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestedStream"/> class.
    /// </summary>
    /// <param name="underlyingStream">The stream to read from.</param>
    /// <param name="length">The number of bytes to read from the parent stream.</param>
    public NestedStream(Stream underlyingStream, long length)
    {
        if (underlyingStream == null) throw new ArgumentNullException(nameof(underlyingStream));
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (!underlyingStream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(underlyingStream));

        this.underlyingStream = underlyingStream;
        remainingBytes = length;
        this.length = length;
    }

    /// <inheritdoc />
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public override bool CanRead => !IsDisposed;

    /// <inheritdoc />
    public override bool CanSeek => !IsDisposed && underlyingStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length
    {
        get
        {
            VerifyNotDisposed();

            return underlyingStream.CanSeek ?
                length : throw new NotSupportedException();
        }
    }

    /// <inheritdoc />
    public override long Position
    {
        get
        {
            VerifyNotDisposed();
            return length - remainingBytes;
        }

        set
        {
            Seek(value, SeekOrigin.Begin);
        }
    }

    /// <inheritdoc />
    public override void Flush() => ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken) => throw ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        VerifyNotDisposed();

        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException();
        }

        count = (int)Math.Min(count, remainingBytes);

        if (count <= 0)
        {
            return 0;
        }

        var bytesRead = await underlyingStream.ReadAsync(buffer, offset, count);
        remainingBytes -= bytesRead;
        return bytesRead;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        VerifyNotDisposed();

        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException();
        }

        count = (int)Math.Min(count, remainingBytes);

        if (count <= 0)
        {
            return 0;
        }

        var bytesRead = underlyingStream.Read(buffer, offset, count);
        remainingBytes -= bytesRead;
        return bytesRead;
    }

#if SPAN_BUILTIN
        /// <inheritdoc />
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            VerifyNotDisposed(this);

            // If we're beyond the end of the stream (as the result of a Seek operation), return 0 bytes.
            if (this.remainingBytes < 0)
            {
                return 0;
            }

            buffer = buffer.Slice(0, (int)Math.Min(buffer.Length, this.remainingBytes));

            if (buffer.IsEmpty)
            {
                return 0;
            }

            int bytesRead = await this.underlyingStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            this.remainingBytes -= bytesRead;
            return bytesRead;
        }
#endif

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        VerifyNotDisposed();

        if (!CanSeek)
        {
            throw new NotSupportedException("The underlying stream does not support seeking.");
        }

        // Recalculate offset relative to the current position
        var newOffset = origin switch
        {
            SeekOrigin.Current => offset,
            SeekOrigin.End => length + offset - Position,
            SeekOrigin.Begin => offset - Position,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin."),
        };

        // Determine whether the requested position is within the bounds of the stream
        if (Position + newOffset < 0)
        {
            throw new IOException("An attempt was made to move the position before the beginning of the stream.");
        }

        var currentPosition = underlyingStream.Position;
        var newPosition = underlyingStream.Seek(newOffset, SeekOrigin.Current);
        remainingBytes -= newPosition - currentPosition;
        return Position;
    }

    /// <inheritdoc />
    public override void SetLength(long value) => throw ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        VerifyNotDisposed();
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        VerifyNotDisposed();
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }

    private Exception ThrowDisposedOr(Exception ex)
    {
        VerifyNotDisposed();
        throw ex;
    }

    private void VerifyNotDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}
