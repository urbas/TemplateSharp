using System;
using System.Text;
using System.IO;

namespace Template.Text
{
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

