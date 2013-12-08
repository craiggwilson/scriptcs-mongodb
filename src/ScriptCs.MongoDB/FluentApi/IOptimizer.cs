using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.MongoDB.FluentApi
{
    internal interface IOptimizer
    {
        bool TryOptimize(List<Op> ops);
    }
}
