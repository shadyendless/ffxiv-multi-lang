using FFXIVMultiLang.Utils;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVMultiLang.Extensions;

public static class UIColorExtensions
{
    public static HaselColor GetForegroundColor(this UIColor row)
        => HaselColor.FromABGR(row.UIForeground);

    public static HaselColor GetEdgeColor(this UIColor row)
        => HaselColor.FromABGR(row.UIGlow);
}
