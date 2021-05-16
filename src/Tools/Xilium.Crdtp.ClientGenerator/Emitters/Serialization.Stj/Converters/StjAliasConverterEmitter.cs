using System.Collections.Generic;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class StjAliasConverterEmitter : StjConverterEmitter
    {
        private readonly AliasTypeInfo _aliasTypeInfo;

        public StjAliasConverterEmitter(Context context, AliasTypeInfo aliasTypeInfo)
            : base(context, aliasTypeInfo)
        {
            _aliasTypeInfo = aliasTypeInfo;
        }

        protected TypeInfo UnderlyingTypeInfo => _aliasTypeInfo.UnderlyingType;

        protected override IEnumerable<CS.SyntaxObject> GetReadBody()
        {
            if (UnderlyingTypeInfo is IntegerTypeInfo)
            {
                yield return new CS.Raw("if (!reader.TryGetInt32(out var value)) throw new JsonException();");
                yield return new CS.Raw($"return new {TypeInfo.GetFullyQualifiedName()}(value);");
            }
            else if (UnderlyingTypeInfo is NumberTypeInfo)
            {
                yield return new CS.Raw("if (!reader.TryGetDouble(out var value)) throw new JsonException();");
                yield return new CS.Raw($"return new {TypeInfo.GetFullyQualifiedName()}(value);");
            }
            else if (UnderlyingTypeInfo is StringTypeInfo)
            {
                yield return new CS.Raw("var value = reader.GetString() ?? throw new JsonException();");
                yield return new CS.Raw($"return new {TypeInfo.GetFullyQualifiedName()}(value);");
            }
            else
            {
                yield return new CS.Raw($"throw new JsonException(\"Not implemented yet.\");");
                throw Error.NotImplemented();
            }
        }

        protected override IEnumerable<CS.SyntaxObject> GetWriteBody()
        {
            if (UnderlyingTypeInfo is IntegerTypeInfo
                || UnderlyingTypeInfo is NumberTypeInfo)
            {
                yield return new CS.Raw("writer.WriteNumberValue(value.GetUnderlyingValue());");
            }
            else if (UnderlyingTypeInfo is StringTypeInfo)
            {
                yield return new CS.Raw("writer.WriteStringValue(value.GetUnderlyingValue());");
            }
            else
            {
                yield return new CS.Raw($"throw new JsonException(\"Not implemented yet.\");");
                throw Error.NotImplemented();
            }
        }
    }
}
