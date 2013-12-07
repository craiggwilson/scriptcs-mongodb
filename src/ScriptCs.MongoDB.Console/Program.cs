using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var mongodb = (MongoDB)new MongoDBScriptPack().GetContext();

            var db = mongodb.Connect("mongodb://localhost");

            var result = db.Command(new BsonDocument("ismaster", 1));
            System.Console.WriteLine(result);

            db.Close();
        }
    }
}
