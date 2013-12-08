using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ScriptCs.MongoDB
{
    internal static class ParameterizingQueryParser
    {
        public static BsonDocument Parse(string json, params object[] parameters)
        {
            return new BsonDocumentWrapper(json, new JsonStringSerializer(JsonReaderSettings.Defaults, parameters));
        }

        private class ParameterizingJsonReader : JsonReader
        {
            private const char PARAMETER_IDENTIFIER = '@';

            private static FieldInfo _currentValueField = typeof(JsonReader)
                .GetField("_currentValue", BindingFlags.Instance | BindingFlags.NonPublic);

            private readonly JsonBuffer _buffer;
            private readonly object[] _parameters;

            public ParameterizingJsonReader(JsonBuffer buffer, JsonReaderSettings settings, object[] parameters)
                : base(buffer, settings)
            {
                _buffer = buffer;
                _parameters = parameters;
            }

            public override BsonType ReadBsonType<TValue>(BsonTrie<TValue> bsonTrie, out bool found, out TValue value)
            {
                try
                {
                    return base.ReadBsonType<TValue>(bsonTrie, out found, out value);
                }
                catch // FileFormatException
                {
                    var currentChar = (char)_buffer.Read();
                    if (currentChar == PARAMETER_IDENTIFIER)
                    {
                        var number = new StringBuilder();
                        currentChar = (char)_buffer.Read();
                        while (Char.IsDigit(currentChar))
                        {
                            number.Append(currentChar);
                            currentChar = (char)_buffer.Read();
                        }
                        _buffer.UnRead((int)currentChar);

                        found = false;
                        value = default(TValue);
                        return ReplaceWithParameter(int.Parse(number.ToString()));
                    }
                    throw;
                }
            }

            private BsonType ReplaceWithParameter(int index)
            {
                if (_parameters == null || _parameters.Length <= index)
                {
                    throw new InvalidOperationException(string.Format("A parameter {0}{1} was found, but the provided parameters are only of length {2}",
                        PARAMETER_IDENTIFIER,
                        index,
                        _parameters == null ? "null" : _parameters.Length.ToString()));
                }

                var replacement = BsonValue.Create(_parameters[index]);
                CurrentBsonType = replacement.BsonType;
                _currentValueField.SetValue(this, replacement);
                State = BsonReaderState.Name;

                return CurrentBsonType;
            }
        }

        private class JsonStringSerializer : BsonBaseSerializer<string>
        {
            private readonly object[] _parameters;
            private readonly JsonReaderSettings _settings;

            public JsonStringSerializer(JsonReaderSettings settings, object[] parameters)
            {
                _settings = settings;
                _parameters = parameters;
            }

            public override void Serialize(BsonSerializationContext context, string value)
            {
                using (var jsonReader = new ParameterizingJsonReader(new JsonBuffer(value), _settings, _parameters))
                {
                    ForwardFromReaderToWriter(null, jsonReader, context.Writer);
                }
            }

            private void ForwardFromReaderToWriter(string name, BsonReader reader, BsonWriter writer)
            {
                if (name != null)
                    writer.WriteName(name);

                switch (reader.GetCurrentBsonType())
                {
                    case BsonType.Array:
                        reader.ReadStartArray();
                        writer.WriteStartArray();
                        while (reader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            ForwardFromReaderToWriter(null, reader, writer);
                        }
                        reader.ReadEndArray();
                        writer.WriteEndArray();
                        break;
                    case BsonType.Binary:
                        writer.WriteBinaryData(reader.ReadBinaryData());
                        break;
                    case BsonType.Boolean:
                        writer.WriteBoolean(reader.ReadBoolean());
                        break;
                    case BsonType.DateTime:
                        writer.WriteDateTime(reader.ReadDateTime());
                        break;
                    case BsonType.Document:
                        reader.ReadStartDocument();
                        writer.WriteStartDocument();
                        while (reader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            ForwardFromReaderToWriter(reader.ReadName(), reader, writer);
                        }
                        reader.ReadEndDocument();
                        writer.WriteEndDocument();
                        break;
                    case BsonType.Double:
                        writer.WriteDouble(reader.ReadDouble());
                        break;
                    case BsonType.Int32:
                        writer.WriteInt32(reader.ReadInt32());
                        break;
                    case BsonType.Int64:
                        writer.WriteInt64(reader.ReadInt64());
                        break;
                    case BsonType.JavaScript:
                        writer.WriteJavaScript(reader.ReadJavaScript());
                        break;
                    case BsonType.JavaScriptWithScope:
                        writer.WriteJavaScriptWithScope(reader.ReadJavaScriptWithScope());
                        break;
                    case BsonType.MaxKey:
                        reader.ReadMaxKey();
                        writer.WriteMaxKey();
                        break;
                    case BsonType.MinKey:
                        reader.ReadMinKey();
                        writer.WriteMinKey();
                        break;
                    case BsonType.Null:
                        reader.ReadNull();
                        writer.WriteNull();
                        break;
                    case BsonType.ObjectId:
                        writer.WriteObjectId(reader.ReadObjectId());
                        break;
                    case BsonType.RegularExpression:
                        writer.WriteRegularExpression(reader.ReadRegularExpression());
                        break;
                    case BsonType.String:
                        writer.WriteString(reader.ReadString());
                        break;
                    case BsonType.Symbol:
                        writer.WriteSymbol(reader.ReadSymbol());
                        break;
                    case BsonType.Timestamp:
                        writer.WriteTimestamp(reader.ReadTimestamp());
                        break;
                    case BsonType.Undefined:
                        reader.ReadUndefined();
                        writer.WriteUndefined();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}