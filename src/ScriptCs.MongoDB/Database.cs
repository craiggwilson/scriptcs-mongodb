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
    public class Database : IDisposable
    {
        private readonly ICluster _cluster;
        private readonly DatabaseNamespace _dbNamespace;
        private readonly ISession _session;
        private bool _disposed;

        public Database(ICluster cluster, DatabaseNamespace dbNamespace)
        {
            _cluster = cluster;
            _dbNamespace = dbNamespace;
            _session = new ClusterSession(_cluster);
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

        public BsonDocument Command(BsonDocument command)
        {
            var commandOp = new GenericCommandOperation<CommandResult>
            {
                Database = _dbNamespace,
                Session = _session,
                Command = command
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