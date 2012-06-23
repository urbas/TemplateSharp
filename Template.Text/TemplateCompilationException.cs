// 
// TemplateCompilationException.cs
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
using Mono.Unix;

namespace Template.Text
{
    /// <summary>
    /// Template compilation exceptions are thrown when a compilation of a string template
    /// fails for any reason (typically because of a malformed template).
    /// This exception provides some explanation to the user.
    /// </summary>
    public class TemplateCompilationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Banshee.Renamer.TemplateCompilationException"/> class.
        /// </summary>
        /// <param name="message">
        /// A human-readable and informative message as to why the template compilation failed.
        /// </param>
        /// <param name="rawPattern">
        /// The template that is the object of this exception.
        /// </param>
        /// <param name="position">
        /// The character position in the `RawPattern` relevant to this exception.
        /// </param>
        /// <param name="cause">
        /// [Optional] The inner exception that may have caused this exception.
        /// </param>
        public TemplateCompilationException (string message, string rawPattern = null, int position = -1, Exception cause = null)
            : base(message, cause)
        {
            if (string.IsNullOrEmpty (message)) {
                throw new Exception (Catalog.GetString ("A non-empty and informative template compilation error message must be provided."));
            }
            RawPattern = rawPattern;
            Position = position;
        }

        /// <summary>
        /// The character position in the `RawPattern` relevant to this exception. May be <c>null</c>.
        /// </summary>
        public readonly int Position;

        /// <summary>
        /// The template that is the object of this exception. May be <c>-1</c> to indicate that the position is unknown.
        /// </summary>
        public readonly string RawPattern;

        public string FullMessage {
            get {
                if (RawPattern == null) {
                    // The message format in case the RawPattern is not provided:
                    if (Position >= 0) {
                        return string.Format(Catalog.GetString("{0} At character {1}."), Message, Position);
                    } else {
                        return Message;
                    }
                } else {
                    // The raw template is provided. Print it out:
                    if (Position >= 0) {
                        return string.Format(Catalog.GetString("{0} In template '{1}' at character {2}."), Message, RawPattern, Position);
                    } else {
                        return string.Format(Catalog.GetString("{0} In template '{1}'."), Message, RawPattern);
                    }
                }
            }
        }
    }
}