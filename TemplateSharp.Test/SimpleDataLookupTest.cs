using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;

namespace Template.Text
{
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

    [TestFixture]
    public class SimpleDataLookupTest
    {
        [Test]
        public void FindReadableProperties_must_return_only_properties_with_public_getters ()
        {
            var expectedProperties = new[] {
                "Title",
                "Year",
                "BoundByName"
            };

            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();

            Assert.AreEqual (readableProperties.Select (property => property.Name), expectedProperties);
        }

        [Test]
        public void FindMemberExplicitlyBoundToParameter_must_find_the_property_and_its_attribute_when_parameter_name_matches ()
        {
            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyAndAttribute = SimpleDataLookup.FindMemberExplicitlyBoundToParameter (readableProperties, "foo bar");

            Assert.AreEqual (propertyAndAttribute.Item1.Name, "Year");
        }

        [Test]
        public void FindMemberExplicitlyBoundToParameter_must_return_null_if_no_property_is_explicitly_bound_to_the_parameter ()
        {
            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyAndAttribute = SimpleDataLookup.FindMemberExplicitlyBoundToParameter (readableProperties, "invalid parameter name");

            Assert.IsNull (propertyAndAttribute);
        }

        [Test]
        public void IsMemberExplicitlyBoundToParameter_must_return_false_for_unbound_property ()
        {
            var propertyInfo = typeof(ClassWithHiddenProperties).GetProperty ("Year");
            var templateParameterAttribute = (TemplateParameter)propertyInfo.GetCustomAttributes (typeof(TemplateParameter), true) [0];

            Assert.IsFalse (SimpleDataLookup.IsMemberExplicitlyBoundToParameter (propertyInfo, templateParameterAttribute, "invalid parameter name"));
        }

        [Test]
        public void IsMemberExplicitlyBoundToParameter_must_return_true_for_bound_property ()
        {
            var propertyInfo = typeof(ClassWithHiddenProperties).GetProperty ("Year");
            var templateParameterAttribute = (TemplateParameter)propertyInfo.GetCustomAttributes (typeof(TemplateParameter), true) [0];

            Assert.IsTrue (SimpleDataLookup.IsMemberExplicitlyBoundToParameter (propertyInfo, templateParameterAttribute, "foo bar"));
        }

        [Test]
        public void IsMemberExplicitlyBoundToParameter_must_return_true_if_the_name_matches ()
        {
            var propertyInfo = typeof(ClassWithHiddenProperties).GetProperty ("BoundByName");
            var templateParameterAttribute = (TemplateParameter)propertyInfo.GetCustomAttributes (typeof(TemplateParameter), true) [0];

            Assert.IsTrue (SimpleDataLookup.IsMemberExplicitlyBoundToParameter (propertyInfo, templateParameterAttribute, "BoundByName"));
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_a_lambda_that_gets_the_property_value ()
        {
            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyLookupLambda = SimpleDataLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties> (readableProperties, "foo bar");

            var dataSourceObject = new ClassWithHiddenProperties () {
                Year = 9001
            };

            Assert.AreEqual (dataSourceObject.Year, propertyLookupLambda (dataSourceObject));
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_a_lambda_that_gets_the_submember_value ()
        {
            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyLookupLambda = SimpleDataLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties> (readableProperties, "BoundByName");

            var dataSourceObject = new ClassWithHiddenProperties () {
                BoundByName = new ClassWithHiddenProperties() {
                    Title = "IT'S OVER 9000!"
                }
            };

            Assert.AreEqual (dataSourceObject.BoundByName.Title, propertyLookupLambda (dataSourceObject));
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_null_for_implicitly_bound_properties ()
        {
            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyLookupLambda = SimpleDataLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties> (readableProperties, "Title");

            Assert.IsNull (propertyLookupLambda);
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_null_for_nonexistent_properties ()
        {
            var readableProperties = SimpleDataLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyLookupLambda = SimpleDataLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties> (readableProperties, "NonExistent");

            Assert.IsNull (propertyLookupLambda);
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_a_lambda_that_returns_the_value_of_a_method ()
        {
            var members = typeof(ClassWithHiddenProperties).GetMembers();
            var propertyLookupLambda = SimpleDataLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties> (members, "SomeFunction");

            var dataSourceObject = new ClassWithHiddenProperties();

            Assert.AreEqual (dataSourceObject.SomeFunction(), propertyLookupLambda(dataSourceObject));
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_a_lambda_that_returns_the_value_of_a_field ()
        {
            var members = typeof(ClassWithHiddenProperties).GetMembers();
            var propertyLookupLambda = SimpleDataLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties> (members, "Goo");

            var dataSourceObject = new ClassWithHiddenProperties();

            Assert.AreEqual (dataSourceObject.Goo, propertyLookupLambda(dataSourceObject));
        }

        [Test]
        public void FindParameterlessFunctions_finds_only_public_method ()
        {
            var foundMethods = SimpleDataLookup.FindParameterlessFunctions<ClassWithHiddenProperties>().Select(method => method.Name);

            Assert.IsTrue(foundMethods.Contains( "SomeFunction"));
            Assert.IsFalse(foundMethods.Contains("HiddenFunction"));
            Assert.IsFalse(foundMethods.Contains("FunctionWithParameters"));
            Assert.IsFalse(foundMethods.Contains("VoidFunction"));
        }
    }
}

