# [![ScriptCs](https://secure.gravatar.com/avatar/5c754f646971d8bc800b9d4057931938?s=200)](http://scriptcs.net/).[![MongoDB](http://info.10gen.com/rs/10gen/images/MongoDB_Logo_Full.png)](http://mongodb.org/)

A [scriptcs](https://github.com/scriptcs/scriptcs) 
[script pack](https://github.com/scriptcs/scriptcs/wiki/Script-Packs-master-list) 
for [MongoDB](https://github.com/mongodb/mongo-csharp-driver).

## Getting Started

Yeah, not done yet.  Currently this is using a pre-alpha fork of the next 
version of the .NET MongoDB driver.  It is pulled in as a git submodule.  I'm mostly using
this project to ensure that the next version of the driver will support different
high-level APIs easily. 

As the next version of the .NET driver stabilizes, this project will be more viable.
For now, it should be considered unstable and pre-alpha.

## Cloning

After cloning this repo, run the following 2 commands:

	git submodule init
	git submodule update

At this point, you'll be able to open the solution file and work with the code.

## Examples

```csharp
var mongo = Require<Mongo>();

// if a database is not specified, it defaults to "test"
var db = mongo.Connect("mongodb://localhost/foo");

var col = db["bar"];

// A fluent-api is built, much as will be present in the 
// next version of all the MongoDB drivers.  Order of
// operations matters, so a skip(10).limit(4) is different
// than limit(4).skip(10). Also, all write operations except 
// for insert come after finding documents.

// Find all the documents in the collection and remove them.
col.Find().Remove();

// Insert 10 documents.
for (int i = 0; i < 10; i++)
	col.Insert(new BsonDocument("x", i * 2));

// Create a view of the collection.  This is a client-side
// view and no queries have been sent to the server.
var view = col.Find("{x: {$gt: 4}}");
Console.WriteLine(view);

// Execute the query by calling AsEnumerable and indicating
// the shape of the results.
foreach(var doc in view.AsEnumerable<BsonDocument>()) 
	Console.WriteLine(doc);

// We can update all the documents in the view right now because
// the query is simply enough.  If anything other than a filter or 
// a limit or 1 had been specified, we could not perform the update
// or remove;

// Increment x by 2 when x > 4.
view.Update("{$inc: {x: 2}}");

// We can add further refinement to the view.  In this case,
// the query is now too complex to render in the normal query
// engine, so we'll execute it as an aggregation framework
// pipeline.
view = view.Skip(4).Sort("{x: 1}");
Console.WriteLine(view);

// And enumerate it exactly the same way.
foreach(var doc in view.AsEnumerable<BsonDocument>()) 
	Console.WriteLine(doc);

// Close up the database to kill the connection pool.
db.Close();
```