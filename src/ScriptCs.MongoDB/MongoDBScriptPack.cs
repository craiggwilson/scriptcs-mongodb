using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.MongoDB
{
    public class MongoDBScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            session.AddReference("MongoDB.Bson.dll");
            session.AddReference("MongoDB.Driver.dll");

            session.ImportNamespace("MongoDB.Bson");
            session.ImportNamespace("MongoDB.Driver");
            session.ImportNamespace("MongoDB.Driver.Builders");
        }

        public IScriptPackContext GetContext()
        {
            return new MongoDB();
        }

        public void Terminate()
        {
        }
    }
}
