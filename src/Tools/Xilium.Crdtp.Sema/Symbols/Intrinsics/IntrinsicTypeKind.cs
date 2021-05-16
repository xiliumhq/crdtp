namespace Xilium.Crdtp.Sema.Symbols
{
    // TODO: Object is DictionaryValue when used as type
    // each value might be
    // TypeNull = 0,
    // TypeBoolean,
    // TypeInteger,
    // TypeDouble,
    // TypeString,
    // TypeBinary,
    // TypeObject,
    // TypeArray,
    // TypeImported ?

    public enum IntrinsicTypeKind
    {
        Any = 1,
        Binary = 2,
        Boolean = 3,
        Integer = 4,
        Number = 5,
        Object = 6,
        String = 7,
    }
}
