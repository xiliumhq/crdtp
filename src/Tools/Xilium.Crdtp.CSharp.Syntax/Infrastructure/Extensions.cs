namespace Xilium.Chromium.DevTools.Syntax
{

    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class Extensions
    {

        public static IEnumerable<T> AsSyntaxList<T>(this IEnumerable<T>? enumerable)
        {
            if (enumerable == null) return Enumerable.Empty<T>();
            else if (enumerable is List<T>) return enumerable;
            else return enumerable.ToList();
        }

        public static StringBuilder AppendWithLeadingSpace(this StringBuilder sb, string value)
        {
            if (sb.Length > 0) sb.Append(' ');
            return sb.Append(value);
        }
    }
}
