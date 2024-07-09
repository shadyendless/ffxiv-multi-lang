using Lumina.Excel.GeneratedSheets;

namespace FFXIVMultiLang.Extensions;

public static class MapExtensions
{
    // "41 0F BF C0 66 0F 6E D0 B8"
    private static uint ConvertRawToMapPos(this Map map, short offset, float value)
    {
        var scale = map.SizeFactor / 100.0f;
        return (uint)(10 - (int)(((value + offset) * scale + 1024f) * -0.2f / scale));
    }

    public static uint ConvertRawToMapPosX(this Map map, float x)
        => ConvertRawToMapPos(map, map.OffsetX, x);

    // tip: you probably want to pass the Z coord
    public static uint ConvertRawToMapPosY(this Map map, float y)
        => ConvertRawToMapPos(map, map.OffsetY, y);
}
