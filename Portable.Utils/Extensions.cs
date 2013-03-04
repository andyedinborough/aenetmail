using System;
using System.Collections.Generic;
using System.Linq;

namespace Portable.Utils
{
    public static class Extensions
    {
        public static IEnumerable<T> GetValues<T>()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("Type must be enumeration type.");

            return GetValues_impl<T>();
        }

        private static IEnumerable<T> GetValues_impl<T>()
        {
            return from field in typeof(T).GetFields()
                   where field.IsLiteral && !string.IsNullOrEmpty(field.Name)
                   select (T)field.GetValue(null);
        }
    }
}
