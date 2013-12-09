using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class SortOp : Op
    {
        public SortOp(BsonDocument sort)
        {
            Sort = sort;
        }

        public override OpType OpType
        {
            get { return OpType.Sort; }
        }

        public BsonDocument Sort { get; private set; }

        public override BsonDocument ToBsonDocument()
        {
            return new BsonDocument("$sort", Sort);
        }
    }
}