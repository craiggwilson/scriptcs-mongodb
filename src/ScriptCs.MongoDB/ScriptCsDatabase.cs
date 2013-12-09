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
    public class ScriptCsDatabase : IDisposable
    {
        private readonly ICluster _cluster;
        private readonly DatabaseNamespace _dbNamespace;
        private readonly ReadPreference _readPreference;
        private readonly ISession _session;
        private readonly WriteConcern _writeConcern;
        private bool _disposed;

        internal ScriptCsDatabase(ICluster cluster, DatabaseNamespace dbNamespace, ReadPreference readPreference, WriteConcern writeConcern)
        {
            if (cluster == null) throw new ArgumentNullException("cluster");
            if (dbNamespace == null) throw new ArgumentNullException("dbNamespace");
            if (readPreference == null) throw new ArgumentNullException("readPreference");
            if (writeConcern == null) throw new ArgumentNullException("writeConcern");

            _cluster = cluster;
            _dbNamespace = dbNamespace;
            _readPreference = readPreference;
            _writeConcern = writeConcern;
            _session = new ClusterSession(_cluster);
        }

        public ScriptCsCollection this[string collectionName]
        {
            get { return GetCollection(collectionName); }
        }

        public void Close()
        {
            if (!_disposed)
            {
                _disposed = true;
                _session.Dispose();
                _cluster.Dispose();
            }
        }

        public ScriptCsCollection GetCollection(string collectionName)
        {
            return new ScriptCsCollection(
                _cluster, 
                _session, 
                new CollectionNamespace(_dbNamespace.DatabaseName, collectionName),
                _readPreference,
                _writeConcern);
        }

        public BsonDocument RunCommand(string command)
        {
            var commandOp = new GenericCommandOperation<CommandResult>
            {
                Database = _dbNamespace,
                Session = _session,
                Command = ParameterizingJsonParser.Parse(command),
                ReadPreference = _readPreference
            };

            var result = commandOp.Execute();
            return result.Response;
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}