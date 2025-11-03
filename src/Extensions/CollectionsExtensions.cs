namespace MessagingDemo.Extensions;

public static class CollectionsExtensions
{
    public static bool AreSetEqual<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        var firstSet = first as ISet<T> ?? first.ToImmutableHashSet();
        var secondSet = second as ISet<T> ?? second.ToImmutableHashSet();

        return firstSet.Count == secondSet.Count && firstSet.SetEquals(secondSet);
    }
}