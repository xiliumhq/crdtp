using System.Linq;

namespace Xilium.Crdtp.Pdl
{
    // TODO: move out this
    public static class PdlTypes
    {
        public const string Any = "any";
        public const string Array = "array";
        public const string Binary = "binary";
        public const string Boolean = "boolean";
        public const string Integer = "integer";
        public const string Number = "number";
        public const string Object = "object";
        public const string String = "string";

        private static readonly string[] List = new[] { Any, Array, Binary, Boolean, Integer, Number, Object, String };

        public static bool Contains(string type)
        {
            return List.Contains(type);
        }
    }
}
