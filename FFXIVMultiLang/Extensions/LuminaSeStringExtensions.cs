using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace FFXIVMultiLang.Extensions;

public static class LuminaSeStringExtensions
{
    public static string ExtractText(this SeString str)
        => new ReadOnlySeStringSpan(str.RawData).ExtractText().Replace("\u00AD", "");
}
