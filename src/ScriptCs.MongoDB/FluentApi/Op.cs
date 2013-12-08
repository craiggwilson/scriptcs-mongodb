using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal abstract class Op
    {
        public abstract OpType OpType { get; }

        public abstract BsonDocument ToBsonDocument();
    }
}
