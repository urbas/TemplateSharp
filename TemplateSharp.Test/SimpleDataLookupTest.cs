using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;

namespace Template.Text
{
    [TestFixture]
    public class SimpleDataLookupTest
    {
        public SimpleDataLookup<ClassWithHiddenProperties> SimpleDataLookup {
            get;
            private set;
        }

        [SetUp]
        public void SetUp()
        {
            SimpleDataLookup = new SimpleDataLookup<ClassWithHiddenProperties>();
        }

        [Test]
        public void CreateValueLookup_returns_a_lambda_that_gets_the_property_value ()
        {
            var propertyLookupLambda = SimpleDataLookup.CreateValueLookup ("foo bar");

            var dataSourceObject = new ClassWithHiddenProperties () {
                Year = 9001
            };

            Assert.AreEqual (dataSourceObject.Year, propertyLookupLambda (dataSourceObject));
        }

        [Test]
        public void CreateValueLookup_returns_a_lambda_that_gets_the_submember_value ()
        {
            var propertyLookupLambda = SimpleDataLookup.CreateValueLookup ("BoundByName");

            var dataSourceObject = new ClassWithHiddenProperties () {
                BoundByName = new ClassWithHiddenProperties() {
                    Title = "IT'S OVER 9000!"
                }
            };

            Assert.AreEqual (dataSourceObject.BoundByName.Title, propertyLookupLambda (dataSourceObject));
        }

        [Test]
        public void CreateValueLookup_returns_a_lambda_that_returns_the_value_of_a_method ()
        {
            var propertyLookupLambda = SimpleDataLookup.CreateValueLookup("SomeFunction");

            var dataSourceObject = new ClassWithHiddenProperties();

            Assert.AreEqual (dataSourceObject.SomeFunction(), propertyLookupLambda(dataSourceObject));
        }

        [Test]
        public void CreateValueLookup_returns_a_lambda_that_returns_the_value_of_a_field ()
        {
            var propertyLookupLambda = SimpleDataLookup.CreateValueLookup("Goo");

            var dataSourceObject = new ClassWithHiddenProperties();

            Assert.AreEqual (dataSourceObject.Goo, propertyLookupLambda(dataSourceObject));
        }

        [Test]
        public void CreateValueLookup_caches_previously_created_lookup ()
        {
            SimpleDataLookup.CreateValueLookup("Goo");

            Assert.IsTrue (SimpleDataLookup.IsLookupCached("Goo"));
        }

        [Test]
        public void IsLookupCached_returns_false_when_lookup_has_not_been_created ()
        {
            Assert.IsFalse (SimpleDataLookup.IsLookupCached("Goo"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateValueLookup_throws_an_exception_when_parameter_cannot_be_bound ()
        {
            SimpleDataLookup.CreateValueLookup("NonExistentMember");
        }
    }
    public class ClassWithHiddenProperties
    {
        public string Title { get; set; }

        [TemplateParameter("foo bar")]
        public int? Year { get; set; }

        [TemplateParameter(Property = "Title")]
        public ClassWithHiddenProperties BoundByName { get; set; }

        public readonly int? TrackCount = 1785;

        [TemplateParameter]
        public readonly int? Goo = 9032;

        public string Hidden {
            set {}
        }

        [TemplateParameter]
        public string SomeFunction ()
        {
            return "What does the scouter say about his power level?";
        }

        private bool HiddenFunction ()
        {
            return true;
        }

        public string FunctionWithParameters (string something)
        {
            return "something foo " + something;
        }

        public void VoidFunction ()
        {
        }
    }
}

