namespace Xilium.Crdtp.Sema.Symbols
{
    public readonly struct QualifiedType
    {
        private readonly TypeSymbol _type;
        private readonly TypeQualifiers _qualifiers;

        public QualifiedType(TypeSymbol type, TypeQualifiers qualifiers)
        {
            // TODO: Check that type is not null
            _type = type;
            _qualifiers = qualifiers;
        }

        public TypeSymbol Type => _type;

        public TypeQualifiers Qualifiers => _qualifiers;

        public bool IsOptional => (_qualifiers & TypeQualifiers.Optional) != 0;

        public override string ToString() // TODO: Debugging Only
        {
            return string.Format("{0}{1}", IsOptional ? "optional " : "", _type?.Name);
        }
    }
}
