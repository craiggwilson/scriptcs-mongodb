using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class SkipOp : Op
    {
        public SkipOp(int count)
        {
            Count = count;
        }

        public override OpType OpType
        {
            get { return OpType.Skip; }
        }

        public int Count { get; private set; }

        public override BsonDocument ToBsonDocument()
        {
            return new BsonDocument("$skip", Count);
        }
    }
}
