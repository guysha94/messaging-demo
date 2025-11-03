


namespace MessagingDemo.Utils;

public static partial class CompiledRegex
{
    [GeneratedRegex(@"[\s\n]+", RegexOptions.Compiled | RegexOptions.IgnoreCase, cultureName: "en-US")]
    public static partial Regex RemoveWhitespaceRegex();
}