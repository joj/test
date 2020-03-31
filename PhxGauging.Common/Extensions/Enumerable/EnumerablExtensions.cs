using System;
using System.Collections.Generic;
using System.Linq;
using PhxGauging.Common.Extensions.String;

namespace PhxGauging.Common.Extensions.Enumerable
{
    /// <summary>
    /// Defines extension methods for Enumerable objects
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Invokes an action for each element in the IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IEnumerable</typeparam>
        /// <param name="iEnum">The IEnumberable object to iterate over.</param>
        /// <param name="action">The action to apply to each element</param>
        public static void Each<T>(this IEnumerable<T> iEnum, Action<T> action)
        {
            foreach (T t in iEnum)
            {
                action(t);
            }
        }

        /// <summary>
        /// Invokes an action for each element in the IEnumerable with the index for each element.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IEnumerable</typeparam>
        /// <param name="iEnum">The IEnumberable object to iterate over.</param>
        /// <param name="action">The action to apply to each element</param>
        public static void Each<T>(this IEnumerable<T> iEnum, Action<T, int> action)
        {
            int count = 0;

            foreach (T t in iEnum)
            {
                action(t, count++);
            }
        }

        /// <summary>
        /// Returns an IEnumerable object without elements that match the given condition.
        /// </summary>
        /// <typeparam name="T">The type of the IEnumerable</typeparam>
        /// <param name="iEnum">The object to iterate over</param>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>Returns an IEnumerable object without elements that match the given condition</returns>
        public static IEnumerable<T> DeleteIf<T>(this IEnumerable<T> iEnum, Predicate<T> condition)
        {
            foreach (T t in iEnum)
            {
                if (!condition(t))
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Returns a string of the given IEnumerable with each element suffixed with the separator
        /// except for the last.
        /// <example>{1, 2, 3, 4}.ToStringJoin("-") => "1-2-3-4"</example>
        /// </summary>
        /// <typeparam name="T">The type of each element in the IEnumerable</typeparam>
        /// <param name="iEnum">The object to iterate over</param>
        /// <param name="separator">The string suffix each element with, except for the last</param>
        /// <returns>Returns a string of the given IEnumerable with each element suffixed with the separator
        /// except for the last
        /// </returns>
        public static string ToStringJoin<T>(this IEnumerable<T> iEnum, string separator)
        {
            string retStr = System.String.Empty;

            if (!iEnum.Any())
                return retStr;

            foreach (T t in iEnum)
            {
                retStr += t.ToString() + separator;
            }

            retStr = separator.IsBlank() ? retStr : retStr.Range(0, -(separator.Length));

            return retStr;
        }

        /// <summary>
        /// Returns a string of the given IEnumerable with each element suffixed with the separator
        /// except for the last.
        /// <example>{1, 2, 3, 4}.ToStringJoin() => "1234"</example>
        /// </summary>
        /// <typeparam name="T">The type of each element in the IEnumerable</typeparam>
        /// <param name="iEnum">The object to iterate over</param>
        /// <returns>Returns a string of the given IEnumerable with each element suffixed with the separator
        /// except for the last
        /// </returns>
        public static string ToStringJoin<T>(this IEnumerable<T> iEnum)
        {
            return iEnum.ToStringJoin(null);
        }
    }

}
