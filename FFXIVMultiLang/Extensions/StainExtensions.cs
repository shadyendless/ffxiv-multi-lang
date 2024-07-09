using System.Buffers.Binary;
using FFXIVMultiLang.Utils;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVMultiLang.Extensions;

public static class StainExtensions
{
    public static HaselColor GetColor(this Stain row)
        => HaselColor.From(BinaryPrimitives.ReverseEndianness(row.Color) >> 8).WithAlpha(1);
}
