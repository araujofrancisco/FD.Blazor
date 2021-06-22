using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FD.Blazor.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Overloads the OrderBy method to accept property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, string propertyName)
        {
            return source.OrderBy(Utils.GetFunc<T>(propertyName));
        }

        /// <summary>
        /// Overloads the OrderBy method to accept property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(Utils.GetExpression<T>(propertyName));
        }

        /// <summary>
        /// Overloads the OrderByDescending method to accept property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, string propertyName)
        {
            return source.OrderByDescending(Utils.GetFunc<T>(propertyName));
        }

        /// <summary>
        /// Overloads the OrderByDescending method to accept property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(Utils.GetExpression<T>(propertyName));
        }

        /// <summary>
        /// Conditional Lambda expression without breaking the flow of the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="condition"></param>
        /// <param name="thenPath"></param>
        /// <param name="elsePath"></param>
        /// <returns></returns>
        public static IEnumerable<T> IfThenElse<T>(
            this IEnumerable<T> elements,
            Func<bool> condition,
            Func<IEnumerable<T>, IEnumerable<T>> thenPath,
            Func<IEnumerable<T>, IEnumerable<T>> elsePath)
        {
            return condition()
                ? thenPath(elements)
                : elsePath(elements);
        }

        /// <summary>
        /// Conditional Lambda expression without breaking the flow of the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="condition"></param>
        /// <param name="thenPath"></param>
        /// <param name="elsePath"></param>
        /// <returns></returns>
        public static IQueryable<T> IfThenElse<T>(
            this IQueryable<T> elements,
            Func<bool> condition,
            Func<IQueryable<T>, IQueryable<T>> thenPath,
            Func<IQueryable<T>, IQueryable<T>> elsePath)
        {
            return condition()
                ? thenPath(elements)
                : elsePath(elements);
        }

        /// <summary>
        /// Returns distinct elements on a enumeration using provided function as comparer.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Returns distinct elements on a IQuaryable using provided function as comparer.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IQueryable<TSource> DistinctBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }
    }
}
