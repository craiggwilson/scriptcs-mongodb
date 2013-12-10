using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.Sessions;

namespace ScriptCs.MongoDB
{
    public class ScriptCsCollection
    {
        private readonly ICluster _cluster;
        private readonly ISession _session;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly ReadPreference _readPreference;
        private readonly WriteConcern _writeConcern;

        internal ScriptCsCollection(ICluster cluster,
            ISession session, 
            CollectionNamespace collectionNamespace,
            ReadPreference readPreference,
            WriteConcern writeConcern)
        {
            if (cluster == null) throw new ArgumentNullException("cluster");
            if (session == null) throw new ArgumentNullException("session");
            if (collectionNamespace == null) throw new ArgumentNullException("collectionNamespace");
            if (readPreference == null) throw new ArgumentNullException("readPreference");
            if (writeConcern == null) throw new ArgumentNullException("writeConcern");

            _cluster = cluster;
            _session = session;
            _collectionNamespace = collectionNamespace;
            _readPreference = readPreference;
            _writeConcern = writeConcern;
        }

        public IEnumerable<BsonDocument> Insert(string document, params string[] documents)
        {
            if (document == null) throw new ArgumentNullException("document");

            BsonDocument[] others = null;
            if (documents != null)
                others = documents.Select(x => JsonParser.Parse(x)).ToArray();

            return Insert(
                JsonParser.Parse(document),
                others);
        }

        public IEnumerable<BsonDocument> Insert<T>(T document, params T[] documents) where T : class
        {
            if (document == null) throw new ArgumentNullException("document");

            var docs = new List<T> { document };
            if(documents != null)
                docs.AddRange(documents);
            
            var insertOp = new InsertOperation<T>
            {
                Collection = _collectionNamespace,
                Session = _session,
                Documents = docs,
                WriteConcern = _writeConcern
            };

            var result = insertOp.Execute();
            if(result != null)
                return result.Select(x => x.Response).ToList();

            return Enumerable.Empty<BsonDocument>();
        }

        public ScriptCsCollectionView Find()
        {
            return new ScriptCsCollectionView(
                _session,
                _collectionNamespace,
                _readPreference,
                _writeConcern);
        }

        public ScriptCsCollectionView Find(string filter)
        {
            return Find().Match(filter);
        }

        public ScriptCsCollectionView Find(BsonDocument filter)
        {
            return Find().Match(filter);
        }
    }
}