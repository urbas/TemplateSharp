// 
// TemplateEngineV1Test.cs
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

#if ENABLE_TESTS

using System;
using NUnit.Framework;
using System.Reflection;

namespace Template.Text
{
    [TestFixture]
	public class TemplateEngineV1Test
    {
        #region Test Templates
        //private const string Template_OK_1 = @"[artist]";
        #endregion

        #region Test Data
        //private LookupMap<object> Data_1_Universal;
        //private LookupMap<object> Data_2_DynamicPropertyLookup;
        #endregion

        #region Test Lifecycle Methods
        public TemplateEngineV1Test()
        {
            /*Data_1_Universal = paramName => (dataObj => "<This is " + (paramName ?? "NONAME") + " of " + (dataObj ?? (object)"NOONE").ToString() + "" + ">");
            Data_2_DynamicPropertyLookup = paramName => ( dataObj => {
                if (dataObj == null)
                    return "<NODATA>";
                //var propInfo = dataObj.GetType().GetProperty(paramName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                return "not yet implemented";
            });*/
        }
        #endregion

        #region Tests
        [Test]
        public void TestTemplates ()
        {
            Console.WriteLine("Test");
        }
        #endregion
    }
}

#endif