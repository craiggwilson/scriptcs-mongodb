﻿using System;
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

            var collection = db["bar"];

            // find all documents and remove them...
            collection.Find().Remove();

            // insert 10 documents
            for (int i = 0; i < 10; i++)
                collection.Insert(new BsonDocument("x", i * 2));

            // skip the first 2 docs, take the next 2 docs, and return the ones
            // where x > 10 && x == 8 
            // hint: we won't find any...
            var view = collection
                .Find()
                .Limit(10)
                .Limit(4)
                .Skip(2)
                .Find("{x: {$gt: @0}}", 10)
                .Find("{x: 8}");

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