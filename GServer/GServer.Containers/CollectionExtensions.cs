using System;
using System.Collections.Generic;

namespace GServer.Containers
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Where<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            foreach (var element in collection)
            {
                if (predicate.Invoke(element))
                {
                    yield return element;
                }
            }
        }
        public static T FirstOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> prediate)
        {
            foreach (var element in collection)
            {
                if (prediate.Invoke(element))
                {
                    return element;
                }
            }
            return default(T);
        }
    }
}