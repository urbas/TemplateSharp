// 
// Templates.cs
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
using System.Collections.Generic;
using Mono.Unix;

namespace Template.Text
{
	/// <summary>
	/// The main usage entry of Template#. This class provides factory methods for template compilation.
	/// </summary>
	public static class Templates
	{
		public static ICompiledTemplate<T> Compile<T> (string template, LookupMap<T> dataLookupMap = null, string templateEngine = null)
		{
			if (dataLookupMap == null) {
                dataLookupMap = new SimpleDataLookup<T>().CreateValueLookup;
			}

			if (templateEngine == null) {
				templateEngine = DefaultEngine;
			}

			// Template engines are generic types. This is why we have to use
			// reflection to create instances.
			Type engineType;
			if (!engines.TryGetValue (templateEngine, out engineType)) {
				throw new ArgumentException (string.Format (Catalog.GetString ("Could not compile the template. Template engine '{0}' is unknown."), templateEngine));
			}
			ITemplateEngine<T> te = (ITemplateEngine<T>)Activator.CreateInstance (engineType.MakeGenericType (typeof(T)));
			return te.CompileTemplate (template, dataLookupMap);
		}

		private static readonly Dictionary<string, Type> engines = new Dictionary<string, Type> ();
		private const string DefaultEngine = TemplateEngineV1<object>.CompilerName;

		static Templates ()
		{
			engines.Add (TemplateEngineV1<object>.CompilerName, typeof(TemplateEngineV1<>));
		}
	}
}

