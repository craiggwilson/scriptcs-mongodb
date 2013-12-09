using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class SortCombiner : IOptimizer
    {
        public bool TryOptimize(List<Op> ops)
        {
            int i = 1;
            Op lastOp;
            Op currentOp;
            bool optimized = false;
            while(i < ops.Count)
            {
                lastOp = ops[i - 1];
                currentOp = ops[i];
                if (lastOp.OpType == OpType.Sort && currentOp.OpType == OpType.Sort)
                {
                    var lastSort = ((SortOp)lastOp).Sort;
                    var currentSort = ((SortOp)currentOp).Sort;

                    // fixing a bug with BsonDocumentWrapper
                    currentSort = new BsonDocument(currentSort.Elements);

                    foreach (var element in lastSort.Elements)
                    {
                        if (!currentSort.Contains(element.Name))
                        {
                            currentSort.Add(element);
                        }
                    }

                    ops[i - 1] = new SortOp(currentSort);
                    ops.RemoveAt(i);
                    optimized = true;
                }
                else i++;
            }

            return optimized;
        }
    }
}