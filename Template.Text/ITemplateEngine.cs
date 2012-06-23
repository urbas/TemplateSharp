// 
// ITemplateEngine.cs
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

namespace Template.Text
{
    /// <summary>
    /// Returns some data for the given object (data source).
    /// </summary>
    public delegate object Lookup<in T>(T dataSource);
    /// <summary>
    /// Returns a data lookup function for the given parameter.
    /// </summary>
    public delegate Lookup<T> LookupMap<in T>(string parameter);

    /// <summary>
    /// Templates are used to generate strings by filling out placeholders (parameters) with the data extracted
    /// from an object through 'parameter lookup functions'.
    /// </summary>
    public interface ITemplateEngine<T>
    {
        /// <summary>
        /// Returns a compiled template from the given string. The returned object will
        /// be used for mass string creation. This is why compiled templates should be
        /// kind of optimised for speed.
        ///
        /// <para>
        /// The data lookup can be used by the compiled template to fetch data from
        /// the data source and thereby fill out its (template's) placeholders (parameters).
        /// </para>
        /// </summary>
        /// <exception cref="Banshee.Renamer.TemplateCompilationException">thrown if the
        /// compilation failed for any reason.</exception>
        ICompiledTemplate<T> CompileTemplate(string template, LookupMap<T> parameterMap);

        /// <summary>
        /// Gets the name of this compiler.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a human-readable (ASCII-art) string that describes the usage
        /// of this template (how to format the string template so that it
        /// properly compiles and does what the user wants).
        /// </summary>
        string Usage { get; }
    }
}

