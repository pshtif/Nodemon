/*
 *	Created by:  Peter @sHTiF Stefcek
 *
 *  The two LINQ conveniences Nodemon used to borrow from OdinSerializer.Utilities.
 *  Instance List<T>.ForEach still wins where it exists; these cover the
 *  .Where(...).ForEach(...) chains over plain IEnumerable<T>.
 */

using System;
using System.Collections.Generic;

namespace Nodemon
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> p_source, Action<T> p_action)
        {
            if (p_source == null) return;
            foreach (var item in p_source) p_action(item);
        }

        public static void AddRange<T>(this ICollection<T> p_target, IEnumerable<T> p_items)
        {
            if (p_items == null) return;
            foreach (var item in p_items) p_target.Add(item);
        }
    }
}
