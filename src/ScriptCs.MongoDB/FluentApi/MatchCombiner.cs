using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class MatchCombiner : IOptimizer
    {
        public bool TryOptimize(List<Op> ops)
        {
            var i = 1;
            Op lastOp;
            Op currentOp;
            var optimized = false;
            while (i < ops.Count)
            {
                lastOp = ops[i - 1];
                currentOp = ops[i];

                if (lastOp.OpType == OpType.Match && currentOp.OpType == OpType.Match)
                {
                    var lastMatch = ((MatchOp)lastOp).Filter;
                    var currentFilter = ((MatchOp)currentOp).Filter;

                    BsonDocument combinedFilter;
                    if (lastMatch.Contains("$and"))
                    {
                        combinedFilter = new BsonDocument(lastMatch.Elements);
                        lastMatch["$and"].AsBsonArray.Add(currentFilter);
                    }
                    else
                    {
                       combinedFilter = new BsonDocument("$and", new BsonArray { ((MatchOp)lastOp).Filter, ((MatchOp)currentOp).Filter });
                    }
                    ops[i - 1] = new MatchOp(combinedFilter);
                    ops.RemoveAt(i);
                    optimized = true;
                }
                else i++;
            }

            return optimized;
        }
    }
}
