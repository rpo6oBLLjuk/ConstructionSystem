using System.Collections.Generic;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}