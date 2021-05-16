using Xilium.Crdtp.Model;

namespace Xilium.Crdtp.Emitters
{
    // TODO(dmitry.azaraev): WellKnownTypes might be part of Context.
    internal sealed class WellKnownTypes
    {
        public Context Context { get; }

        // External
        public WellKnownTypeInfo StjSerializerOptionsTypeInfo { get; }
        public WellKnownTypeInfo JsonConverterTypeInfo { get; }

        // Generated
        public WellKnownTypeInfo ProtocolApiTypeInfo { get; }
        public WellKnownTypeInfo ProtocolStjSerializerOptionsTypeInfo { get; }

        public WellKnownTypes(Context context)
        {
            Context = context;

            // External
            StjSerializerOptionsTypeInfo = new WellKnownTypeInfo(context,
                "StjSerializerOptions",
                "Xilium.Crdtp.Client.Serialization");

            JsonConverterTypeInfo = new WellKnownTypeInfo(context,
                "JsonConverter",
                "System.Text.Json.Serialization");

            // Generated
            ProtocolApiTypeInfo = new WellKnownTypeInfo(context,
                Context.Options.ProtocolApiTypeName,
                Context.Options.Namespace);

            ProtocolStjSerializerOptionsTypeInfo = new WellKnownTypeInfo(context,
                "ProtocolStjSerializerOptions",
                Context.NamingPolicy.GetNamespaceName(null as string)
                );
        }

        public WellKnownOf1TypeInfo GetIEnumerableOf(TypeInfo typeInfo)
        {
            return new WellKnownOf1TypeInfo(Context,
                "IEnumerable",
                "System.Collections.Generic",
                typeInfo);
        }

        public WellKnownOf1TypeInfo GetICollectionOf(TypeInfo typeInfo)
        {
            return new WellKnownOf1TypeInfo(Context,
                "ICollection",
                "System.Collections.Generic",
                typeInfo);
        }

        public WellKnownOf1TypeInfo GetIEquatableOf(TypeInfo typeInfo)
        {
            return new WellKnownOf1TypeInfo(Context,
                "IEquatable",
                "System",
                typeInfo);
        }

        public WellKnownOf1TypeInfo GetListOf(TypeInfo typeInfo)
        {
            return new WellKnownOf1TypeInfo(Context,
                "List",
                "System.Collections.Generic",
                typeInfo);
        }

        public WellKnownOf1TypeInfo GetJsonConverterOf(TypeInfo typeInfo)
        {
            return new WellKnownOf1TypeInfo(Context,
                JsonConverterTypeInfo.Name,
                JsonConverterTypeInfo.Namespace,
                typeInfo);
        }
    }
}
