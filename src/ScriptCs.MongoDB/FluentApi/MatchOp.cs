using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class MatchOp : Op
    {
        public MatchOp(BsonDocument filter)
        {
            Filter = filter;
        }

        public override OpType OpType
        {
            get { return OpType.Match; }
        }

        public BsonDocument Filter { get; private set; }

        public override BsonDocument ToBsonDocument()
        {
            return new BsonDocument("$match", Filter);
        }
    }
}
