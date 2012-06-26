// 
// ICompiledTemplate.cs
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
using System.Text;
using System.IO;

namespace Template.Text
{
    /// <summary>
    /// Templates are strings that contain placeholders which in turn are supposed to be replaced by data extracted from a given object (the data source).
    /// Compiled templates do exactly this but they have been compiled from one such 'template string' by a template engine for efficiency purposes.
    /// </summary>
    public interface ICompiledTemplate<in T>
    {
        /// <summary>
        /// Gets the 'template string' that gave rise to this compiled pattern.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Creates a string by filling out placeholders with data provided by the data source.
        /// This method builds the string with the given string builder. The same string builder
        /// can be cleared after a call to this method and reused in calling this method again.
        /// Doing this can greatly increase performance (when compared to the `string CreateString(T)`
        /// method).
        /// </summary>
        void CreateString(StringBuilder output, T dataSource);

        /// <summary>
        /// Returns a string by filling out the placeholders in this template with the data
        /// provided by the `dataSource` object.
        /// </summary>
        string CreateString(T dataSource);

        /// <summary>
        /// Creates a string by filling out placeholders with data provided by the data source.
        /// This method builds the string into the given text writer (useful for writing strings
        /// to streams and similar).
        /// </summary>
        void CreateString(TextWriter output, T dataSource);
    }

    /// <summary>
    /// A simple and stupid implementation of the compiled template interface.
    /// Can be used by implementors for simplicity sake.
    /// </summary>
    public abstract class CompiledTemplate<T> : ICompiledTemplate<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Template.Text.CompiledTemplate`1"/> class.
        /// </summary>
        /// <param name='source'>
        /// The string form of the template.
        /// </param>
        public CompiledTemplate(string source)
        {
            Source = source;
        }

        public string Source {
            get;
            private set;
        }

        public virtual void CreateString (StringBuilder output, T dataSource)
        {
            CreateString(new StringWriter(output), dataSource);
        }

        public virtual string CreateString (T dataSource)
        {
            StringWriter sw = new StringWriter();
            CreateString(sw, dataSource);
            return sw.ToString();
        }

        public abstract void CreateString (TextWriter output, T dataSource);
    }
}

