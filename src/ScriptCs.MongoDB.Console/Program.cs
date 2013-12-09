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
            var mongodb = (Mongo)new MongoScriptPack().GetContext();

            var db = mongodb.Connect("mongodb://localhost/foo");

            var result = db.RunCommand("{ismaster: 1}");
            System.Console.WriteLine(result);

            var collection = db["bar"];

            // find all documents and remove them...
            collection.Find().Remove();

            // insert 10 documents
            for (int i = 0; i < 10; i++)
                collection.Insert(new BsonDocument("x", i * 2));

            var view = collection
                .Find()
                .Match("{x: {$gt: @0}}", 10)
                .Sort("{x: 1}")
                .Match("{x: 8}")
                .Sort("{a: 1}")
                .Limit(10)
                .Limit(4)
                .Skip(2);

            System.Console.WriteLine(view);

            foreach(var doc in view.AsEnumerable<BsonDocument>())
                System.Console.WriteLine(doc);

            System.Console.WriteLine();
            System.Console.WriteLine("Press <any> key to exit.");
            System.Console.ReadKey();

            db.Close();
        }
    }
}