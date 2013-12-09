using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.MongoDB
{
    public class MongoScriptPack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            session.AddReference("MongoDB.Bson.dll");
            session.AddReference("MongoDB.Driver.Core.dll");

            session.ImportNamespace("MongoDB.Bson");
            session.ImportNamespace("ScriptCs.MongoDB");
        }

        public IScriptPackContext GetContext()
        {
            return new Mongo();
        }

        public void Terminate()
        {
        }
    }
}
