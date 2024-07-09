namespace FFXIVMultiLang.Extensions;

public static class StringExtensions
{
    //! https://stackoverflow.com/a/4405876
    public static string FirstCharToUpper(this string input)
        => string.IsNullOrEmpty(input) ? string.Empty : string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1));

    public static string FirstCharToLower(this string input)
        => string.IsNullOrEmpty(input) ? string.Empty : string.Concat(input[0].ToString().ToLower(), input.AsSpan(1));
}
