using System;
using System.Text.Json;
using Xilium.Crdtp.Client.Dispatching;

namespace Xilium.Crdtp.Client.Serialization
{
    internal static class DispatchableParser
    {
        // TODO(dmitry.azaraev): (Low) This should be true shallow parser for JSON, but need some benchmarks first.
        // TODO: Use ValueTextEquals(ReadOnlySpan<byte> utf8Text) instead of string
        public static Dispatchable Parse(ReadOnlySpan<byte> message)
        {
            // TODO(dmitry.azaraev): Do shallow parsing to Dispatchable and run dispatch with client.
            // Response
            // {"id":1,"result":{}}
            // {"id":1,"result":{},"sessionId":"71A2FEA71FBBC53F270580AA45E78504"}
            // {"id":2,"result":{"frameId":"1CA20D801E5F2F1D9C9D03E87864900C","loaderId":"9828C1D773594017B807FA99F0D8E945"},"sessionId":"D59AC82ABB84CBD12AB88F14AA51A236"}
            // {"id":2,"error":{"code":-32601,"message":"'Page.navigate2' wasn't found"},"sessionId":"0305043331889088A7CACBED400AC64E"}
            // Notification
            // {"method":"Page.frameStartedLoading","params":{"frameId":"D9B53DA7BD83417CFB8D10F7A2C2141F"},"sessionId":"32B4866411BA78B98D505E3C94BB217B"}

            var jsonReader = new Utf8JsonReader(message, StjOptions.ReaderOptions);

            if (!jsonReader.Read()) throw Error.InvalidOperation(); // parsing error
            Check.That(jsonReader.TokenType == JsonTokenType.StartObject);

            int? callId = default;
            string? sessionId = default;
            string? method = default;
            Dispatchable.PayloadType dataType = Dispatchable.PayloadType.None;
            ReadOnlySpan<byte> data = default;

            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (jsonReader.ValueTextEquals("id")) // TODO: use pre-encoded values
                        {
                            Check.That(jsonReader.Read());
                            callId = jsonReader.GetInt32();
                            continue;
                        }
                        else if (jsonReader.ValueTextEquals("sessionId"))
                        {
                            Check.That(jsonReader.Read());
                            sessionId = jsonReader.GetString();
                            continue;
                        }
                        else if (jsonReader.ValueTextEquals("method"))
                        {
                            Check.That(jsonReader.Read());
                            method = jsonReader.GetString();
                            continue;
                        }
                        else if (jsonReader.ValueTextEquals("result"))
                        {
                            Check.That(dataType == Dispatchable.PayloadType.None);
                            dataType = Dispatchable.PayloadType.Result;
                            var startOffset = checked((int)jsonReader.BytesConsumed);
                            Check.That(jsonReader.Read());
                            jsonReader.Skip();
                            var endOffset = checked((int)jsonReader.BytesConsumed);
                            data = message.Slice(startOffset, endOffset - startOffset);
                        }
                        else if (jsonReader.ValueTextEquals("error"))
                        {
                            Check.That(dataType == Dispatchable.PayloadType.None);
                            dataType = Dispatchable.PayloadType.Error;
                            var startOffset = checked((int)jsonReader.BytesConsumed);
                            Check.That(jsonReader.Read());
                            jsonReader.Skip();
                            var endOffset = checked((int)jsonReader.BytesConsumed);
                            data = message.Slice(startOffset, endOffset - startOffset);
                        }
                        else if (jsonReader.ValueTextEquals("params"))
                        {
                            Check.That(dataType == Dispatchable.PayloadType.None);
                            dataType = Dispatchable.PayloadType.Params;
                            var startOffset = checked((int)jsonReader.BytesConsumed);
                            Check.That(jsonReader.Read());
                            jsonReader.Skip();
                            var endOffset = checked((int)jsonReader.BytesConsumed);
                            data = message.Slice(startOffset, endOffset - startOffset);
                        }
                        else throw Error.InvalidOperation("Uknown property name.");
                        break;

                    case JsonTokenType.EndObject:
                        Check.That(!jsonReader.Read());
                        break;

                    default:
                        throw Error.InvalidOperation();
                }
            }

            // TODO(dmitry.azaraev): Verify what created Dispatchable is correct, e.g. there is response or notification, or throw protocol violation.

            return new Dispatchable(sessionId, callId, method, dataType, data);
        }
    }
}
