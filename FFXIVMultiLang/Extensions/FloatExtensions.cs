namespace FFXIVMultiLang.Extensions;

public static class FloatExtensions
{
    public static bool IsApproximately(this float a, float b, float margin = 0.001f)
        => Math.Abs(a - b) < margin;
}
