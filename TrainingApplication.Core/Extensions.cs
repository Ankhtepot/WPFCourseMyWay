using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainingApplication.Core
{
    public static class Extensions
    {
        public static bool IsGenericEnumerableType(this Type type)
        {
            return type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static string ToHumanReadable(this string value)
        {
            return string.Concat(value.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
    }
}