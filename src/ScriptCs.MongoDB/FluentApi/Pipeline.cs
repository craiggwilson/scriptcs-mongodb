﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class Pipeline
    {
        private static IOptimizer[] _optimizers = new IOptimizer[]
        {
            new MatchCombiner(),
            new SkipLimitCombiner()
        };

        private List<Op> _ops;

        public Pipeline()
        {
            _ops = new List<Op>();
        }

        private Pipeline(IEnumerable<Op> ops)
        {
            _ops = ops.ToList();
        }

        public Pipeline AddLimit(int count)
        {
            var copy = Copy();
            copy.Push(new LimitOp(count));
            return copy;
        }

        public Pipeline AddMatch(BsonDocument filter)
        {
            var copy = Copy();
            copy.Push(new MatchOp(filter));
            return copy;
        }

        public Pipeline AddSkip(int count)
        {
            var copy = Copy();
            copy.Push(new SkipOp(count));
            return copy;
        }

        public BsonDocument[] ToBsonDocumentArray()
        {
            return _ops.Select(x => x.ToBsonDocument()).ToArray();
        }

        public bool TryGetQueryArgs(out QueryArgs args)
        {
            args = new QueryArgs();
            foreach (var op in _ops)
            {
                switch (op.OpType)
                {
                    case OpType.Match:
                        if (args.Limit != null) goto default;
                        if (args.Skip != null) goto default;
                        if (args.Filter != null) goto default;
                        args.Filter = ((MatchOp)op).Filter;
                        break;
                    case OpType.Limit:
                        if (args.Limit != null) goto default;
                        if (args.Filter != null) goto default;
                        args.Limit = ((LimitOp)op).Count;
                        break;
                    case OpType.Skip:
                        if (args.Limit != null) goto default;
                        if (args.Skip != null) goto default;
                        if (args.Filter != null) goto default;
                        args.Skip = ((SkipOp)op).Count;
                        break;
                    default:
                        args = null;
                        return false;
                }
            }

            return true;
        }

        private Pipeline Copy()
        {
            return new Pipeline(_ops);
        }

        private void Optimize()
        {
            foreach (var optimizer in _optimizers)
            {
                optimizer.TryOptimize(_ops);
            }
        }

        private void Push(Op op)
        {
            _ops.Add(op);
            Optimize();
        }
    }
}