namespace SekaiLayer.Extensions;

public static class EnumerableExtension
{
    public static bool Contains<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        return list.FirstOrDefault(predicate) is not null;
    }
}