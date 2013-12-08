using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class SkipLimitCombiner : IOptimizer
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

                if (lastOp.OpType == OpType.Limit && currentOp.OpType == OpType.Limit)
                {
                    var min = Math.Min(((LimitOp)lastOp).Count, ((LimitOp)currentOp).Count);
                    ops[i - 1] = new LimitOp(min);
                    ops.RemoveAt(i);
                    optimized = true;
                }
                else if(lastOp.OpType == OpType.Skip && currentOp.OpType == OpType.Skip)
                {
                    var added = ((SkipOp)lastOp).Count + ((SkipOp)currentOp).Count;
                    ops[i - 1] = new SkipOp(added);
                    ops.RemoveAt(i);
                    optimized = true;
                }
                else if(lastOp.OpType == OpType.Limit && currentOp.OpType == OpType.Skip)
                {
                    var subtracted = ((LimitOp)lastOp).Count - ((SkipOp)currentOp).Count;

                    if(subtracted > 0)
                    {
                        ops[i - 1] = currentOp;
                        ops[i] = new LimitOp(subtracted);
                        optimized = true;
                    }
                    else
                    {
                        // the results will be 0... it is sufficient to state that
                        // this can't be sent using normal query syntax and let
                        // agg framework process this and return 0 results.
                    }
                }
                else i++;
            }

            return optimized;
        }
    }
}