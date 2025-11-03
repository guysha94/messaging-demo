namespace MessagingDemo.Extensions;

public static class StringExtensions
{
    public static string ToKebabCase(this string s) =>
        string.Concat(s.Select((ch, i) =>
            char.IsUpper(ch) && i > 0 ? $"-{char.ToLowerInvariant(ch)}" : char.ToLowerInvariant(ch).ToString()));
}