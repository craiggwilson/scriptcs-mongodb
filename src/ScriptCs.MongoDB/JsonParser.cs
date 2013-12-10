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
    internal static class JsonParser
    {
        public static BsonDocument Parse(string json)
        {
            return new BsonDocumentWrapper(json, new JsonStringSerializer(JsonReaderSettings.Defaults));
        }

        private class JsonStringSerializer : BsonBaseSerializer<string>
        {
            private readonly JsonReaderSettings _settings;

            public JsonStringSerializer(JsonReaderSettings settings)
            {
                _settings = settings;
            }

            public override void Serialize(BsonSerializationContext context, string value)
            {
                using (var jsonReader = new JsonReader(new JsonBuffer(value), _settings))
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