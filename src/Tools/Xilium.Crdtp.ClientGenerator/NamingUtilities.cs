using System.Text;

namespace Xilium.Crdtp
{
    internal static class NamingUtilities
    {
        public static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (char.IsLower(value, 0))
                return char.ToUpperInvariant(value[0]) + value[1..];

            return value;
        }

        /// <summary>
        /// Converts a kebab-case to pascal case (E.g., first-line -> FirstLine)
        /// </summary>
        /// <param name="value"></param>
        public static string KebabToPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var parts = value.Split('-');
            var sb = new StringBuilder();
            foreach (var p in parts)
            {
                sb.Append(Capitalize(p));
            }
            return sb.ToString();
        }
    }
}
