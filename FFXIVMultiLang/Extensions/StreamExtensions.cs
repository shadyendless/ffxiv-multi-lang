using System.IO;
using FFXIVMultiLang.Utils;
using Lumina.Text.Expressions;

namespace FFXIVMultiLang.Extensions;

public static class StreamExtensions
{
    /// Copyright (c) Andrew Arnott. All rights reserved.<br/>
    /// Licensed under the MIT license. See https://github.com/dotnet/Nerdbank.Streams/blob/00fded/LICENSE for full license information. 
    /// <summary>
    /// Creates a <see cref="Stream"/> that can read no more than a given number of bytes from an underlying stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="length">The number of bytes to read from the parent stream.</param>
    /// <returns>A stream that ends after <paramref name="length"/> bytes are read.</returns>
    public static Stream ReadSlice(this Stream stream, long length) => new NestedStream(stream, length);

    public static void CopyStreamWithLengthTo(this Stream other, Stream stream)
    {
        IntegerExpression.EncodeStatic(stream, (uint)other.Length);
        other.Position = 0;
        other.CopyTo(stream);
    }
}
