using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class QueryArgs
    {
        public BsonDocument Filter;
        public int? Limit;
        public BsonDocument OrderBy;
        public int? Skip;
    }
}
