using System;
using System.Collections.Generic;
using System.Linq;
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
        public static BsonDocument Parse(string json)
        {
            using(var reader = BsonReader.Create(json))
            {
                var context = BsonDeserializationContext.CreateRoot<BsonDocumentWriterSettings>(reader);
                return new BsonDocumentSerializer().Deserialize(context);
            }
        }
    }
}