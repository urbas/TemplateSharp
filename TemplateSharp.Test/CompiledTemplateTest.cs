using System;
using System.Text;
using NUnit.Framework;

namespace Template.Text
{
    [TestFixture]
    public class CompiledTemplateTest
    {
        const string FooBar = "foo bar";

        private FakeCompiledTemplate fakeCompiledTemplate;

        class FakeCompiledTemplate : CompiledTemplate<Object>
        {
            public FakeCompiledTemplate (string template) : base(template)
            {
            }

            public override void CreateString (System.IO.TextWriter output, object dataSource)
            {
                output.Write(FooBar);
                output.Write(dataSource.ToString());
            }
        }

        [SetUp]
        public void SetUp ()
        {
            fakeCompiledTemplate = new FakeCompiledTemplate (FooBar);
        }

        [Test]
        public void Source_returns_the_template_string_set_in_constructor ()
        {
            Assert.AreEqual(FooBar, fakeCompiledTemplate.Source);
        }

        [Test]
        public void CreateString_returns_the_template_with_toString_of_object_appended ()
        {
            var testDataSource = "zar mar";

            Assert.AreEqual(FooBar + testDataSource, fakeCompiledTemplate.CreateString(testDataSource));
        }

        [Test]
        public void CreateString_with_stringBuilder_returns_the_template_string_with_toString_of_object_appended ()
        {
            var testDataSource = "zar mar";
            var stringBuilder = new StringBuilder();

            fakeCompiledTemplate.CreateString(stringBuilder, testDataSource);

            Assert.AreEqual(FooBar + testDataSource, stringBuilder.ToString());
        }
    }
}

