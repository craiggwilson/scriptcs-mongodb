using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class ProjectOp : Op
    {
        public ProjectOp(BsonDocument project)
        {
            Project = project;
        }

        public override OpType OpType
        {
            get { return OpType.Project; }
        }

        public BsonDocument Project { get; private set; }

        public override BsonDocument ToBsonDocument()
        {
            return new BsonDocument("$project", Project);
        }
    }
}