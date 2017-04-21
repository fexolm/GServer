using System;
using System.Collections.Generic;
using System.Linq;
namespace GServer.Containers
{
    public static class CollectionExtensions
    {
        public static void Invoke<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach(var element in collection)
            {
                action.Invoke(element);
            }
        }
    }
}