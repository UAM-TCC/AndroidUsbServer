using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidUsbServer.Common
{
    public static class Extensions
    {
        public static string ToErrorString(this Exception ex) => ex.GetType().FullName + ": " + ex.Message +
            "\nInner: " + ex.InnerException?.Message + "\nStack: " + ex.StackTrace + "\n";

        public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
        {

            first = list.Count > 0 ? list[0] : default(T); // or throw
            rest = list.Skip(1).ToList();
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest)
        {
            first = list.Count > 0 ? list[0] : default(T); // or throw
            second = list.Count > 1 ? list[1] : default(T); // or throw
            rest = list.Skip(2).ToList();
        }
    }
}
