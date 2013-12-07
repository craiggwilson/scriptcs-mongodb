using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

using MongoDB.Driver.Core;

namespace ScriptCs.MongoDB
{
    public class MongoDB : IScriptPackContext
    {
        public ScriptCsDatabase Connect(string connectionString)
        {
            var config = new ClusterConfiguration();
            var connString = new ConnectionString(connectionString);
            config.ConfigureWithConnectionString(connString);

            var cluster = config.BuildCluster();
            return new ScriptCsDatabase(
                cluster,
                new DatabaseNamespace(connString.DatabaseName ?? "test"),
                GetReadPreference(connString),
                GetWriteConcern(connString));
        }

        private ReadPreference GetReadPreference(ConnectionString connString)
        {
            var mode = connString.ReadPreference;
            var tags = connString.ReadPreferenceTags;

            return new ReadPreference(
                connString.ReadPreference ?? ReadPreferenceMode.Primary,
                connString.ReadPreferenceTags);
        }

        private WriteConcern GetWriteConcern(ConnectionString connString)
        {
            return new WriteConcern
            {
                W = connString.W,
                WTimeout = connString.WTimeout,
                FSync = connString.FSync,
                Journal = connString.Journal,
            };
        }
    }
}