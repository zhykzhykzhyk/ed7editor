using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    class ExtensionAttribute : Attribute { }

}

namespace ED7Editor
{

    public delegate TResult Func<TResult>();
    public delegate TResult Func<T, TResult>(T arg);
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate void Action();
    public delegate void Action<T>(T arg);
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public static class Enumerable
    {
        public static TSource Aggregate<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, TSource> func
            )
        {
            bool first = true;
            var e = source.GetEnumerator();

        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func
        )
        {
            return Aggregate(source, seed, func, o => o);
        }


        public static TResult Aggregate<TSource, TAccumulate, TResult>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TResult> resultSelector
        )
        {
            foreach (var item in source)
                seed = func(seed, item);
            return resultSelector(seed);
        }

        public static bool All<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            foreach (var item in source)
                if (!predicate(item)) return false;
            return true;
        }
        public static bool Any<TSource>(
            this IEnumerable<TSource> source
        )
        {
            return Any(source, o => true);
        }
        public static bool Any<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
        )
        {
            foreach (var item in source)
                if (predicate(item)) return true;
            return false;
        }
        public static IEnumerable<TSource> AsEnumerable<TSource>(
            this IEnumerable<TSource> source
        )
        {
            return source;
        }
        public static IEnumerable<TResult> Cast<TResult>(
            this IEnumerable source
            )
        {
            foreach (var item in source)
                yield return (TResult)item;
        }
        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
            )
        {
            foreach (var item in first)
                yield return item;
            foreach (var item in second)
                yield return item;
        }
        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value
            )
        {
            return Contains(source, value, EqualityComparer<TSource>.Default);
        }
        public static bool Contains<TSource>(
            this IEnumerable<TSource> source,
            TSource value,
            IEqualityComparer<TSource> comparer
            )
        {
            return Any(source, c => comparer.Equals(c, value));
        }
        public static int Count<TSource>(
            this IEnumerable<TSource> source
            )
        {
            return Count(source, o => true);
        }
        public static int Count<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
            )
        {
            int count = 0;
            foreach (var item in source) 
                if (predicate(item)) ++count;
            return count;
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source
        )
        {
            return DefaultIfEmpty(source, default(TSource));
        }
        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
            this IEnumerable<TSource> source,
            TSource defaultValue
            )
        {
            bool empty = true;
            foreach (var item in source)
            {
                empty = false;
                yield return item;
            }
            if (empty)
                yield return defaultValue;
        }
        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source
            )
        {
            return Distinct(source, EqualityComparer<TSource>.Default);
        }
        public static IEnumerable<TSource> Distinct<TSource>(
            this IEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer
            )
        {
            Dictionary<TSource, bool> set = new Dictionary<TSource, bool>(comparer);
            foreach (var item in source)
            {
                if (!set.ContainsKey(item))
                {
                    set.Add(item, true);
                    yield return item;
                }
            }
        }
        public static TSource ElementAt<TSource>(
            this IEnumerable<TSource> source,
            int index)
        {
        }
        public static TSource ElementAtOrDefault<TSource>(
            this IEnumerable<TSource> source,
            int index
        )
        {
            try
            {
                return ElementAt(source, index);
            }
            catch (ArgumentOutOfRangeException)
            {
                return default(TSource);
            }
        }
        public static IEnumerable<TResult> Empty<TResult>()
        {
            if (false) yield return default(TResult);
        }
        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second
            )
        {
            Except(first, second, EqualityComparer<TSource>.Default);
        }
        public static IEnumerable<TSource> Except<TSource>(
            this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer
            )
        {
            Dictionary<TSource, bool> set = new Dictionary<TSource, bool>(comparer);
            foreach (var item in second)
                if (!set.ContainsKey(item)) set.Add(item, true);
            foreach (var item in first)
                if (!set.ContainsKey(item)) yield return item;
        }
        public static TSource First<TSource>(
            this IEnumerable<TSource> source
            )
        {
            return First(source, o => true);
        }
        public static TSource First<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
            )
        {
            foreach (var item in source)
            {
                if (predicate(item)) return item;
            }
            throw new InvalidOperationException();
        }
        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source
            )
        {
            return FirstOrDefault(source, o => true);
        }
        public static TSource FirstOrDefault<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate
            )
        {
            try
            {
                return First(source, predicate);
            }
            catch (InvalidOperationException)
            {
                return default(TSource);
            }
        }

    }
}
