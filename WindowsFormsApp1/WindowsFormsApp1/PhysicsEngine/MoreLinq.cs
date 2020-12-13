using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public static class MoreLinq
    {
        public static T MinBy<T, E, U>(this E items, Func<T, U> selector) 
            where T : class
            where E : IEnumerable<T>
            where U : IComparable<U>
        {
            return MinBy(items, selector, Comparer<U>.Default.Compare);
        } 
        public static T MinBy<T, E, U>(this E items, Func<T, U> selector, Comparison<U> comparison)
            where T : class
            where E : IEnumerable<T>
        {
            return BestBy(items, selector, (a, b) => comparison(a, b) < 0);
        }
        public static T MaxBy<T, E, U>(this E items, Func<T, U> selector) 
            where T : class
            where E : IEnumerable<T>
            where U : IComparable<U>
        {
            return MaxBy(items, selector, Comparer<U>.Default.Compare);
        } 
        public static T MaxBy<T, E, U>(this E items, Func<T, U> selector, Comparison<U> comparison)
            where T : class
            where E : IEnumerable<T>
        {
            return BestBy(items, selector, (a, b) => comparison(a, b) > 0);
        }
        
        public static T BestBy<T, E, U>(this E items, Func<T, U> selector, Func<U, U, bool> firstBetter)
            where T : class
            where E : IEnumerable<T>
        {
            using (IEnumerator<T> enumerator = items.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
                T bestItem = enumerator.Current;
                U bestValue = selector(bestItem);
                while (enumerator.MoveNext())
                {
                    T currItem = enumerator.Current;
                    U currValue = selector(currItem);
                    if (firstBetter(currValue, bestValue))
                    {
                        bestItem = currItem;
                        bestValue = currValue;
                    }
                }
                return bestItem;
            }
        }

    }
}