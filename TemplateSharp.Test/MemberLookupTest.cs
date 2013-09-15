// 
// SimpleDataLookup.cs
// 
// Author:
//   Matej Urbas <matej.urbas@gmail.com>
// 
// Copyright 2012 Matej Urbas
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using NUnit.Framework;

namespace Template.Text
{
    [TestFixture]
    public class MemberLookupTest
    {
        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_null_for_implicitly_bound_properties ()
        {
            var readableProperties = MemberLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyLookupLambda = MemberLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties>(readableProperties, "Title");

            Assert.IsNull (propertyLookupLambda);
        }

        [Test]
        public void CreateLookupLambdaForMemberExplicitlyBoundToParameter_returns_null_for_nonexistent_properties ()
        {
            var readableProperties = MemberLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyLookupLambda = MemberLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<ClassWithHiddenProperties>(readableProperties, "NonExistent");

            Assert.IsNull (propertyLookupLambda);
        }

        [Test]
        public void FindReadableProperties_must_return_only_properties_with_public_getters ()
        {
            var expectedProperties = new[] {
                "Title",
                "Year",
                "BoundByName"
            };

            var readableProperties = MemberLookup.FindReadableProperties<ClassWithHiddenProperties> ();

            Assert.AreEqual (readableProperties.Select (property => property.Name), expectedProperties);
        }

        [Test]
        public void FindMemberExplicitlyBoundToParameter_must_find_the_property_and_its_attribute_when_parameter_name_matches ()
        {
            var readableProperties = MemberLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyAndAttribute = MemberLookup.FindMemberExplicitlyBoundToParameter (readableProperties, "foo bar");

            Assert.AreEqual (propertyAndAttribute.Item1.Name, "Year");
        }

        [Test]
        public void FindMemberExplicitlyBoundToParameter_must_return_null_if_no_property_is_explicitly_bound_to_the_parameter ()
        {
            var readableProperties = MemberLookup.FindReadableProperties<ClassWithHiddenProperties> ();
            var propertyAndAttribute = MemberLookup.FindMemberExplicitlyBoundToParameter (readableProperties, "invalid parameter name");

            Assert.IsNull (propertyAndAttribute);
        }

        [Test]
        public void IsMemberExplicitlyBoundToParameter_must_return_false_for_unbound_property ()
        {
            var propertyInfo = typeof(ClassWithHiddenProperties).GetProperty ("Year");
            var templateParameterAttribute = (TemplateParameter)propertyInfo.GetCustomAttributes (typeof(TemplateParameter), true) [0];

            Assert.IsFalse (MemberLookup.IsMemberExplicitlyBoundToParameter (propertyInfo, templateParameterAttribute, "invalid parameter name"));
        }

        [Test]
        public void IsMemberExplicitlyBoundToParameter_must_return_true_for_bound_property ()
        {
            var propertyInfo = typeof(ClassWithHiddenProperties).GetProperty ("Year");
            var templateParameterAttribute = (TemplateParameter)propertyInfo.GetCustomAttributes (typeof(TemplateParameter), true) [0];

            Assert.IsTrue (MemberLookup.IsMemberExplicitlyBoundToParameter (propertyInfo, templateParameterAttribute, "foo bar"));
        }

        [Test]
        public void IsMemberExplicitlyBoundToParameter_must_return_true_if_the_name_matches ()
        {
            var propertyInfo = typeof(ClassWithHiddenProperties).GetProperty ("BoundByName");
            var templateParameterAttribute = (TemplateParameter)propertyInfo.GetCustomAttributes (typeof(TemplateParameter), true) [0];

            Assert.IsTrue (MemberLookup.IsMemberExplicitlyBoundToParameter (propertyInfo, templateParameterAttribute, "BoundByName"));
        }

        [Test]
        public void FindParameterlessFunctions_finds_only_public_method ()
        {
            var foundMethods = MemberLookup.FindParameterlessFunctions<ClassWithHiddenProperties>().Select(method => method.Name);

            Assert.IsTrue(foundMethods.Contains( "SomeFunction"));
            Assert.IsFalse(foundMethods.Contains("HiddenFunction"));
            Assert.IsFalse(foundMethods.Contains("FunctionWithParameters"));
            Assert.IsFalse(foundMethods.Contains("VoidFunction"));
        }
    }
}
