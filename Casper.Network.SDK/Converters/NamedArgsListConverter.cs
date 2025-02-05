using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Casper.Network.SDK.Converters
{
    public class NamedArgsListConverter<T, TConverter> : JsonConverter<List<T>>
    {
        public override List<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Array/Object start token expected to deserialize a list of NamedArgs");

            reader.Read(); // Start object

            var named = reader.GetString();
            reader.Read();
            if (named != "Named")
                throw new JsonException("Named property expected");

            reader.Read(); // Start array

            List<T> list = new List<T>();

            if (reader.TokenType == JsonTokenType.EndObject)
                return list; //this is an empty list, just return...

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                //this is an empty list, skip end array item and return...
                reader.Read();
                return list;
            }

        var tConverter = Activator.CreateInstance(typeof(TConverter)) as JsonConverter;

            if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.StartObject or JsonTokenType.PropertyName)
            {
                while (reader.TokenType == JsonTokenType.StartObject ||
                       reader.TokenType == JsonTokenType.StartArray ||
                       (tConverter is IDeserializeAsList && reader.TokenType == JsonTokenType.PropertyName))
                {
                    var element = JsonSerializer.Deserialize<T>(ref reader, new JsonSerializerOptions()
                    {
                        Converters = { tConverter }
                    });
                    reader.Read(); // end object/array

                    list.Add(element);
                }
            }
            else
            {
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    var element = JsonSerializer.Deserialize<T>(ref reader, new JsonSerializerOptions()
                    {
                        Converters = { tConverter }
                    });
                    list.Add(element);
                    reader.Read();
                }
            }

            reader.Read(); //read end object
            return list;
        }

        public override void Write(
            Utf8JsonWriter writer,
            List<T> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Named");
            writer.WriteStartArray();
            var tConverter = Activator.CreateInstance(typeof(TConverter)) as JsonConverter;
            var elOptions = new JsonSerializerOptions()
            {
                WriteIndented = options.WriteIndented,
                Converters = { tConverter }
            };
            foreach (var element in value)
            {
                JsonSerializer.Serialize(writer, element, elOptions);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}