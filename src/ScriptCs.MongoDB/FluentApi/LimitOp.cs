using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class LimitOp : Op
    {
        public LimitOp(int count)
        {
            Count = count;
        }

        public override OpType OpType
        {
            get { return OpType.Limit; }
        }

        public int Count { get; private set; }

        public override BsonDocument ToBsonDocument()
        {
            return new BsonDocument("$limit", Count);
        }
    }
}
