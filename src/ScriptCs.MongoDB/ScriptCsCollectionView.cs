using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Sessions;
using ScriptCs.MongoDB.FluentApi;

namespace ScriptCs.MongoDB
{
    public class ScriptCsCollectionView
    {
        private readonly Pipeline _pipeline;
        private readonly ISession _session;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly ReadPreference _readPreference;
        private readonly WriteConcern _writeConcern;

        internal ScriptCsCollectionView(
            ISession session, 
            CollectionNamespace collectionNamespace,
            ReadPreference readPreference,
            WriteConcern writeConcern)
            : this(session, collectionNamespace, readPreference, writeConcern, new Pipeline())
        {
        }

        private ScriptCsCollectionView(
            ISession session,
            CollectionNamespace collectionNamespace,
            ReadPreference readPreference,
            WriteConcern writeConcern,
            Pipeline pipeline)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (collectionNamespace == null) throw new ArgumentNullException("collectionNamespace");
            if (readPreference == null) throw new ArgumentNullException("readPreference");
            if (writeConcern == null) throw new ArgumentNullException("writeConcern");
            if (pipeline == null) throw new ArgumentNullException("pipeline");

            _session = session;
            _collectionNamespace = collectionNamespace;
            _readPreference = readPreference;
            _writeConcern = writeConcern;
            _pipeline = pipeline;
        }

        public IEnumerable<T> AsEnumerable<T>() where T : class
        {
            QueryArgs args;
            if (_pipeline.TryGetQueryArgs(out args))
            {
                var queryOp = new QueryOperation<T>
                {
                    Collection = _collectionNamespace,
                    Session = _session,
                    Limit = args.Limit ?? 0,
                    Query = args.Filter ?? new BsonDocument(),
                    Skip = args.Skip ?? 0
                };

                return queryOp;
            }
            else
            {
                var aggregateOp = new AggregateOperation<T>
                {
                    Collection = _collectionNamespace,
                    Session = _session,
                    Pipeline = _pipeline.ToBsonDocumentArray()
                };

                return aggregateOp;
            }

            throw new NotSupportedException("Agg Framework isn't supported yet.");
        }

        public ScriptCsCollectionView Find(string filter, params object[] parameters)
        {
            return new ScriptCsCollectionView(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern,
                _pipeline.AddMatch(ParameterizingQueryParser.Parse(filter, parameters)));
        }

        public ScriptCsCollectionView Limit(int count)
        {
            return new ScriptCsCollectionView(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern,
                _pipeline.AddLimit(count));
        }

        public BsonDocument Remove()
        {
            QueryArgs args;
            if(!_pipeline.TryGetQueryArgs(out args))
            {
                throw new NotSupportedException("The currently defined pipeline does not support removal.");
            }

            if(args.Limit.HasValue && args.Limit.Value != 1)
            {
                throw new NotSupportedException("Limit must either be unspecified or equal 1 when removing documents.");
            }

            if(args.Skip.HasValue)
            {
                throw new NotSupportedException("Skip cannot be specified when removing documents.");
            }

            var removeOp = new RemoveOperation
            {
                Collection = _collectionNamespace,
                Session = _session,
                Query = args.Filter ?? new BsonDocument(),
                IsMulti = !args.Limit.HasValue,
                WriteConcern = _writeConcern
            };

            return removeOp.Execute().Response;
        }

        public T SingleOrDefault<T>() where T : class
        {
            return Limit(1).AsEnumerable<T>().FirstOrDefault();
        }

        public ScriptCsCollectionView Skip(int count)
        {
            return new ScriptCsCollectionView(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern,
                _pipeline.AddSkip(count));
        }

        public override string ToString()
        {
            QueryArgs args;
            if(_pipeline.TryGetQueryArgs(out args))
            {
                var ops = new List<string>();
                if(args.Skip.HasValue)
                    ops.Add("skip(" + args.Skip + ")");
                if (args.Limit.HasValue)
                    ops.Add("limit(" + args.Limit + ")");
                ops.Add("find(" + args.Filter ?? new BsonDocument() + ")");

                return string.Join(".", ops);
            }

            return "aggregate(" + new BsonArray(_pipeline.ToBsonDocumentArray()) + ")";
        }
    }
}