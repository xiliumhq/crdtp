using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.Crdtp.Sema.Symbols;

namespace Xilium.Crdtp.Model
{
    public sealed class PropertyInfo : MemberInfo
    {
        private readonly ObjectTypeInfo _containingType;
        private readonly PropertySymbol _propertySymbol;
        private TypeInfo? _lazyType;

        internal PropertyInfo(Context context, ObjectTypeInfo containingType, PropertySymbol propertySymbol)
            : base(context)
        {
            _containingType = containingType;
            _propertySymbol = propertySymbol;
            context.AddPropertySymbol(propertySymbol, this);
        }

        public override DomainInfo? Domain => _containingType.Domain;

        public ObjectTypeInfo ContainingType => _containingType;

        public override string Name => Context.NamingPolicy.GetPropertyName(_propertySymbol);

        public override string ProtocolName => _propertySymbol.Name;

        public override bool IsReachable => false;

        public override bool IsExperimental => _propertySymbol.IsExperimental;

        public override bool IsDeprecated => _propertySymbol.IsDeprecated;

        public override ImmutableArray<string> Description => _propertySymbol.Description;

        public bool IsOptional => _propertySymbol.Type.IsOptional;

        public TypeInfo Type => _lazyType ?? GetLazyType();

        private TypeInfo GetLazyType()
        {
            return _lazyType = Context.GetTypeInfo(_propertySymbol.Type.Type);
        }
    }
}
