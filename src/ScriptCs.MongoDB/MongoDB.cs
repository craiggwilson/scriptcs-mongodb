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
        public Database Connect(string connectionString)
        {
            var config = new ClusterConfiguration();
            var connString = new ConnectionString(connectionString);
            config.ConfigureWithConnectionString(connString);

            var cluster = config.BuildCluster();
            return new Database(
                cluster,
                new DatabaseNamespace(connString.DatabaseName ?? "test"));
        }
    }
}