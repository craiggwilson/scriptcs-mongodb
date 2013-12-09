using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class MatchSortReorderer : IOptimizer
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

                if (lastOp.OpType == OpType.Sort && currentOp.OpType == OpType.Match)
                {
                    ops[i - 1] = currentOp;
                    ops[i] = lastOp;
                    optimized = true;
                    i = 1; // go back to the start in case
                }
                else i++;
            }

            return optimized;
        }
    }
}