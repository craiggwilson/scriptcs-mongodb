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

namespace ScriptCs.MongoDB
{
    public class ScriptCsCollectionView<T>
    {
        private readonly ViewArgs _args;
        private readonly ISession _session;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly ReadPreference _readPreference;
        private readonly WriteConcern _writeConcern;

        internal ScriptCsCollectionView(
            ISession session, 
            CollectionNamespace collectionNamespace,
            ReadPreference readPreference,
            WriteConcern writeConcern)
            : this(session, collectionNamespace, readPreference, writeConcern, new ViewArgs())
        {
        }

        private ScriptCsCollectionView(
            ISession session,
            CollectionNamespace collectionNamespace,
            ReadPreference readPreference,
            WriteConcern writeConcern,
            ViewArgs args)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (collectionNamespace == null) throw new ArgumentNullException("collectionNamespace");
            if (readPreference == null) throw new ArgumentNullException("readPreference");
            if (writeConcern == null) throw new ArgumentNullException("writeConcern");
            if (args == null) throw new ArgumentNullException("args");

            _session = session;
            _collectionNamespace = collectionNamespace;
            _readPreference = readPreference;
            _writeConcern = writeConcern;
            _args = args;
        }

        public IEnumerable<T> AsEnumerable()
        {
            var queryOp = new QueryOperation<T>
            {
                Collection = _collectionNamespace,
                Session = _session,
                Limit = _args.Take ?? 0,
                Query = _args.Filter,
                Skip = _args.Skip ?? 0
            };

            return queryOp;
        }

        public ScriptCsCollectionView<T> Find(string filter, params object[] parameters)
        {
            var args = _args.Copy();
            args.Filter = ParameterizingQueryParser.Parse(filter, parameters);
            return new ScriptCsCollectionView<T>(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern,
                args);
        }

        public T SingleOrDefault()
        {
            return Take(1).AsEnumerable().FirstOrDefault();
        }

        public ScriptCsCollectionView<T> Skip(int skip)
        {
            var args = _args.Copy();
            args.Skip = skip;
            return new ScriptCsCollectionView<T>(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern,
                args);
        }

        public ScriptCsCollectionView<T> Take(int take)
        {
            var args = _args.Copy();
            args.Take = take;
            return new ScriptCsCollectionView<T>(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern,
                args);
        }

        internal class ViewArgs
        {
            public BsonDocument Filter;
            public int? Skip;
            public int? Take;

            public ViewArgs Copy()
            {
                return new ViewArgs
                {
                    Filter = Filter == null ? null : (BsonDocument)Filter.DeepClone(),
                    Skip = Skip,
                    Take = Take
                };
            }
        }
    }
}