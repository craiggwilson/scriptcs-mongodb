using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace ScriptCs.MongoDB.FluentApi
{
    internal class ProjectOp : Op
    {
        private static readonly string[] _queryEngineProjectionOperators = new[]
        {
            "$elemMatch",
            "$slice"
        };

        public ProjectOp(BsonDocument project)
        {
            Project = project;
            DetermineRequiresAggregationFramework();
        }

        public override OpType OpType
        {
            get { return OpType.Project; }
        }

        public BsonDocument Project { get; private set; }

        public bool RequiresAggregationFramework { get; private set; }

        public override BsonDocument ToBsonDocument()
        {
            return new BsonDocument("$project", Project);
        }

        private void DetermineRequiresAggregationFramework()
        {
            foreach (var element in Project.Elements)
            {
                if(element.Value.IsBsonDocument)
                {
                    var value = (BsonDocument)element.Value;
                    if (value.ElementCount == 0 || !_queryEngineProjectionOperators.Contains(value.GetElement(0).Name))
                        RequiresAggregationFramework = true;
                }
                else if (!element.Value.IsNumeric && !element.Value.IsBoolean)
                    RequiresAggregationFramework = true;
            }
        }
    }
}