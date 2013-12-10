using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Connections;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ScriptCs.MongoDB.FluentApi
{
    public class PipelineTests
    {
        private readonly ScriptCsDatabase _db;
        private readonly ScriptCsCollection _col;

        public PipelineTests()
        {
            _db = new ScriptCsDatabase(
                Substitute.For<ICluster>(),
                new DatabaseNamespace("foo"),
                ReadPreference.Primary,
                WriteConcern.Acknowledged);
            _col = _db["bar"];
        }

        [Fact]
        public void One_match()
        {
            var subject = _col.Find("{x:1}").ToString();

            var expected = "find({ \"x\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Two_adjacent_matches_should_be_combined()
        {
            var subject = _col.Find("{x:1}").Match("{a:1}").ToString();

            var expected = "find({ \"$and\" : [{ \"x\" : 1 }, { \"a\" : 1 }] })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Three_adjacent_matches_should_be_combined()
        {
            var subject = _col.Find("{x:1}").Match("{a:1}").Match("{b:1}").ToString();

            var expected = "find({ \"$and\" : [{ \"x\" : 1 }, { \"a\" : 1 }, { \"b\" : 1 }] })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void One_skip()
        {
            var subject = _col.Find().Skip(1).ToString();

            var expected = "find({ }).skip(1)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Two_adjacent_skips_should_be_added()
        {
            var subject = _col.Find().Skip(1).Skip(12).ToString();

            var expected = "find({ }).skip(13)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Three_adjacent_skips_should_be_added()
        {
            var subject = _col.Find().Skip(1).Skip(12).Skip(4).ToString();

            var expected = "find({ }).skip(17)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void One_limit()
        {
            var subject = _col.Find().Limit(1).ToString();

            var expected = "find({ }).limit(1)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Two_adjacent_limits_should_be_minimized()
        {
            var subject = _col.Find().Limit(5).Limit(3).ToString();

            var expected = "find({ }).limit(3)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Three_adjacent_limits_should_be_minimized()
        {
            var subject = _col.Find().Limit(5).Limit(3).Limit(12).ToString();

            var expected = "find({ }).limit(3)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void One_sort()
        {
            var subject = _col.Find().Sort("{x:1}").ToString();

            var expected = "find({ }).sort({ \"x\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Two_adajenct_sorts_with_different_fields_should_be_combined()
        {
            var subject = _col.Find().Sort("{x:1}").Sort("{a:1}").ToString();

            var expected = "find({ }).sort({ \"a\" : 1, \"x\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Two_adajenct_sorts_with_the_same_fields_should_be_combined()
        {
            var subject = _col.Find().Sort("{x:1}").Sort("{x:1}").ToString();

            var expected = "find({ }).sort({ \"x\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Two_adajenct_sorts_with_the_same_fields_in_different_directions_should_be_combined()
        {
            var subject = _col.Find().Sort("{x:1}").Sort("{x:-1}").ToString();

            var expected = "find({ }).sort({ \"x\" : -1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Three_adajenct_sorts_should_be_combined()
        {
            var subject = _col.Find().Sort("{x:1}").Sort("{a:1}").Sort("{x:1}").ToString();

            var expected = "find({ }).sort({ \"x\" : 1, \"a\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Sort_then_match_should_be_reordered()
        {
            var subject = _col.Find().Sort("{x:1}").Match("{a:1}").ToString();

            var expected = "find({ \"a\" : 1 }).sort({ \"x\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Sort_then_match_then_sort_should_be_reordered_and_the_sorts_combined()
        {
            var subject = _col.Find().Sort("{x:1}").Match("{a:1}").Sort("{x:-1}").ToString();

            var expected = "find({ \"a\" : 1 }).sort({ \"x\" : -1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Match_then_sort_then_match_should_be_reordered_and_the_matches_combined()
        {
            var subject = _col.Find("{x:1}").Sort("{x:1}").Match("{a:1}").ToString();

            var expected = "find({ \"$and\" : [{ \"x\" : 1 }, { \"a\" : 1 }] }).sort({ \"x\" : 1 })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Limit_then_skip_should_be_combined()
        {
            var subject = _col.Find().Limit(10).Skip(4).ToString();

            var expected = "find({ }).skip(4).limit(6)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Limit_then_skip_then_skip_should_be_combined()
        {
            var subject = _col.Find().Limit(10).Skip(4).Skip(2).ToString();

            var expected = "find({ }).skip(6).limit(4)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Project_then_skip_should_be_reordered()
        {
            var subject = _col.Find().Project("{a:1}").Skip(10).ToString();

            var expected = "find({ }, { \"a\" : 1 }).skip(10)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Limit_then_project_then_skip_should_be_reordered()
        {
            var subject = _col.Find().Limit(20).Project("{a:1}").Skip(10).ToString();

            var expected = "find({ }, { \"a\" : 1 }).skip(10).limit(10)";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Project_with_elemMatch_can_be_query()
        {
            var subject = _col.Find().Project("{a: {$elemMatch: {k: 1 }}}").ToString();

            var expected = "find({ }, { \"a\" : { \"$elemMatch\" : { \"k\" : 1 } } })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Project_with_slice_can_be_query()
        {
            var subject = _col.Find().Project("{a: {$slice: 5}}").ToString();

            var expected = "find({ }, { \"a\" : { \"$slice\" : 5 } })";
            subject.Should().Be(expected);
        }

        [Fact]
        public void Project_then_match_cannot_be_a_query()
        {
            var subject = _col.Find().Project("{a:1}").Match("{x:1}").ToString();

            AssertPipeline(subject,
                "{ \"$project\" : { \"a\" : 1 } }",
                "{ \"$match\" : { \"x\" : 1 } }");
        }

        [Fact]
        public void Match_then_project_then_match_cannot_be_a_query()
        {
            var subject = _col.Find("{a:1}").Project("{a:1}").Match("{x:1}").ToString();

            AssertPipeline(subject,
                "{ \"$match\" : { \"a\" : 1 } }",
                "{ \"$project\" : { \"a\" : 1 } }",
                "{ \"$match\" : { \"x\" : 1 } }");
        }

        [Fact]
        public void Skip_then_match_cannot_be_a_query()
        {
            var subject = _col.Find().Skip(10).Match("{x:1}").ToString();

            AssertPipeline(subject,
                "{ \"$skip\" : 10 }",
                "{ \"$match\" : { \"x\" : 1 } }");
        }

        [Fact]
        public void Project_with_rename_cannot_be_a_query()
        {
            var subject = _col.Find().Project("{a: \"b\"}").ToString();

            AssertPipeline(subject,
                "{ \"$project\" : { \"a\" : \"b\" } }");
        }

        public void AssertPipeline(string subject, params string[] operators)
        {
            var combined = "aggregate([" + string.Join(", ", operators) + "])";

            subject.Should().Be(combined);
        }
    }
}