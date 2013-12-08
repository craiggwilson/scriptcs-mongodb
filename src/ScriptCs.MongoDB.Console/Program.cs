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

            var db = mongodb.Connect("mongodb://localhost/foo");

            var result = db.RunCommand("{ismaster: 1}");
            System.Console.WriteLine(result);

            var collection = db.GetCollection<BsonDocument>("bar");

            // find all documents and remove them...
            collection.Find().Remove();

            // insert 10 documents
            for (int i = 0; i < 10; i++)
                collection.Insert(new BsonDocument("x", i * 2));

            // find all documents where x > 10, skip 2, and take 3
            var view = collection
                .Find("{x: {$gt: @0}}", 10)
                .Skip(2)
                .Take(3);

            foreach(var doc in view.AsEnumerable())
            {
                System.Console.WriteLine(doc);
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Press <any> key to exit.");
            System.Console.ReadKey();

            db.Close();
        }
    }
}
