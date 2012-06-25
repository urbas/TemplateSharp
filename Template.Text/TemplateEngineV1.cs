//
// TemplateEngineV1.cs
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
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Mono.Unix;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace Template.Text
{
    #region TemplateV1 Engine
    /// <summary>
    /// A simple `template engine` implementation.
    ///
    /// <para>
    /// Here is an example pattern for this creator:
    /// <code>
    ///     "/some/directories/[artist]/[album]/[FC<{0:00} - >track number][artist] - [album] ([S<00>year]) - [title]"
    /// </code>
    /// </para>
    /// </summary>
    public class TemplateEngineV1<T> : ITemplateEngine<T>
    {
        public const string CompilerName = "TemplateV1";

        public ICompiledTemplate<T> CompileTemplate (string pattern, LookupMap<T> dataLookupMap)
        {
            return new TemplateV1<T> (pattern, dataLookupMap);
        }

        public string Name {
            get {
                return CompilerName;
            }
        }

        public string Usage {
            get {
                return string.Format (Catalog.GetString (
                    "Template engine: `{0}`\n" +
                    "\n" +
                    "The pattern may include template placeholders with the following syntax:\n" +
                    "\n" +
                    "-   [parameter name]: simple parameter (it is interpolated with track's info directly).\n" +
                    "-   [F<format>parameter name]: formatted parameter. Uses .NET's format string.\n" +
                    "-   [F?<format>parameter name]: same as above but not interpolated if parameter is not given for the song.\n" +
                    "-   [C<format>P1,P2,...]: complex formatted parameter. Uses  .NET's composite format syntax.\n" +
                    "-   [C?<format>P1,P2,...]: same as above but not interpolated if parameter is not given for the song.\n" +
                    "\n" +
                    "Some examples:\n" +
                    "\n" +
                    @"-   [home]/Music/[artist] - [album] - [title]" + "\n" +
                    @"-   [directory]/[F<00>track number] - [artist] - [album] - [title]" + "\n" +
                    @"-   [directory]/[C?<{{0:00}} - >track number][artist] - [album] - [title]"
                    ), Name);
            }
        }
    }

    public class TemplateV1<T> : CompiledTemplate<T>
    {
        #region Fields
        private List<TemplateSegmentV1<T>> segments;
        public const char EscapeChar = '\\';
        #endregion

        #region Constructors
        public TemplateV1 (string template, LookupMap<T> dataLookupMap)
            : base(template)
        {
            TemplateCompilerV1<T> compiler = new TemplateCompilerV1<T> (template, dataLookupMap);
            compiler.Compile ();
            this.segments = compiler.Segments;
        }
        #endregion

        #region ICompiledTemplate Implementation
        public override void CreateString (StringBuilder output, T dataSource)
        {
            int segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++) {
                segments [i].ToStringAppend (output, dataSource);
            }
        }

        public override void CreateString (TextWriter output, T dataSource)
        {
            int segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++) {
                segments [i].ToStringAppend (output, dataSource);
            }
        }
        #endregion
    }
    #endregion

    #region TemplateV1 Segments
    /// <summary>
    /// A TemplateV1 template consists of a linear sequence of segments. When creating strings
    /// from the template segments are simply concatenated together.
    /// There are two basic types of TemplateV1 segments: literal segments and placeholder segments.
    /// Literal segments are pure text segments and do not get interpolated with data from
    /// the data source. Placeholders, on the other hand, are interpolated before they are concatenated.
    /// Interpolation in this context means that data are fetched from the data source and placed
    /// on the right places of the placeholder to produce a string. Placeholders are essentially
    /// parametrised by the data source.
    /// </summary>
    public interface ITemplateSegmentV1<in T>
    {
        string ParentTemplate { get; }

        int IndexInTemplate { get; }

        string ToString (T dataSource);

        void ToStringAppend (StringBuilder output, T dataSource);

        void ToStringAppend (TextWriter output, T dataSource);
    }

    public abstract class TemplateSegmentV1<T> : ITemplateSegmentV1<T>
    {
        public TemplateSegmentV1 (string parentTemplate, int indexInTemplate)
        {
            ParentTemplate = parentTemplate;
            IndexInTemplate = indexInTemplate;
        }

        public string ParentTemplate { get; private set; }

        public int IndexInTemplate { get; private set; }

        public abstract string ToString (T dataSource);

        public virtual void ToStringAppend (StringBuilder output, T dataSource)
        {
            output.Append (ToString (dataSource));
        }

        public virtual void ToStringAppend (TextWriter output, T dataSource)
        {
            output.Write (ToString (dataSource));
        }

        protected TemplateCompilationException CreateException (string message, Exception cause = null)
        {
            return new TemplateCompilationException (message, ParentTemplate, IndexInTemplate, cause);
        }
    }

    public sealed class LiteralSegmentV1<T> : TemplateSegmentV1<T>
    {
        public LiteralSegmentV1 (string rawContent, string parentTemplate, int indexInTemplate)
            : base(parentTemplate, indexInTemplate)
        {
            RawContent = rawContent;
        }

        public override string ToString (T dataSource)
        {
            return RawContent;
        }

        public override void ToStringAppend (StringBuilder output, T dataSource)
        {
            output.Append (RawContent);
        }

        public override void ToStringAppend (TextWriter output, T dataSource)
        {
            output.Write (RawContent);
        }

        public string RawContent { get; private set; }
    }

    /// <summary>
    /// A placeholder in TemplateV1 looks like this:
    /// -   [parameter name part].
    /// -   [header part|parameter name part].
    /// -   [header part&lt;format part&gt;parameter name part].
    /// All parts of the parameter support the following two escape sequences: '\\' and '\]'. Character '[' is
    /// valid anywhere within the placeholder.
    /// The 'header part' may be empty and may not contain characters '|' or '&lt;' (they can be escaped via '\|' and '\&lt;').
    /// The 'format part' may be empty and not contain the character '&lt;' (which can be escaped via '\&lt;').
    /// The 'parameter name part' may be empty and is a ',' (comma) separated list of simple parameter names.
    /// </summary>
    public abstract class PlaceholderV1<T> : TemplateSegmentV1<T>
    {
        public const char PlaceholderStartChar = '[';
        public const char PlaceholderEndChar = ']';
        public const char FormatStartChar = '<';
        public const char FormatEndChar = '>';
        public const char HeaderParametersDelimiter = '|';
        public const char ParameterDelimiter = ',';

        protected PlaceholderV1 (string parentTemplate, int indexInTemplate, LookupMap<T> dataLookupMap, string headerPart, string formatPart, params string[] parameterNamePart)
            : base(parentTemplate, indexInTemplate)
        {
            DataLookupMap = dataLookupMap;
            Header = headerPart;
            Format = formatPart;
            if (parameterNamePart == null || parameterNamePart.Length < 1) {
                throw CreateException (Catalog.GetString ("A placeholder requires at least one parameter name."));
            }
            RawParameterNames = parameterNamePart;
            ParameterLookups = new Lookup<T>[parameterNamePart.Length];
            for (int i = 0; i < parameterNamePart.Length; i++) {
                ParameterLookups [i] = dataLookupMap (parameterNamePart [i]);
                if (ParameterLookups [i] == null) {
                    throw CreateException (string.Format (Catalog.GetString ("Unknown parameter '{0}'."), parameterNamePart [i]));
                }
            }
        }

        public LookupMap<T> DataLookupMap { get; private set; }

        public string Header { get; private set; }

        public string Format { get; private set; }

        protected string[] RawParameterNames { get; private set; }

        public ReadOnlyCollection<string> ParameterNames {
            get {
                return new ReadOnlyCollection<string> (RawParameterNames);
            }
        }

        protected Lookup<T>[] ParameterLookups { get; private set; }

        /// <summary>
        /// This is a factory method. It returns the right placeholer based on its header.
        /// </summary>
        public static PlaceholderV1<T> CreatePlaceholder (string parentTemplate, int indexInTemplate, LookupMap<T> dataLookupMap, string headerPart, string formatPart, params string[] parameterNames)
        {
            PlaceholderCreatorV1 ph;
            headerPart = headerPart ?? "";
            if (placeholderTypeRegistry.TryGetValue (headerPart, out ph)) {
                return ph (parentTemplate, indexInTemplate, dataLookupMap, headerPart, formatPart, parameterNames);
            } else {
                throw new TemplateCompilationException (string.Format (Catalog.GetString ("Unknown placeholder type '{0}'."), headerPart), parentTemplate, indexInTemplate);
            }
        }

        private delegate PlaceholderV1<T> PlaceholderCreatorV1 (string parentTemplate,int indexInTemplate,LookupMap<T> dataLookupMap,string headerPart,string formatPart,params string[] parameterNames);

        private static readonly Dictionary<string, PlaceholderCreatorV1> placeholderTypeRegistry = new Dictionary<string, PlaceholderCreatorV1> ();

        static PlaceholderV1 ()
        {
            placeholderTypeRegistry.Add ("", (pt, i, dlm, h, f, ps) => new SimplePlaceholderV1<T> (pt, i, dlm, h, f, ps));
            placeholderTypeRegistry.Add ("F", (pt, i, dlm, h, f, ps) => new SimpleFormatPlaceholderV1<T> (false, pt, i, dlm, h, f, ps));
            placeholderTypeRegistry.Add ("F?", (pt, i, dlm, h, f, ps) => new SimpleFormatPlaceholderV1<T> (true, pt, i, dlm, h, f, ps));
            placeholderTypeRegistry.Add ("C", (pt, i, dlm, h, f, ps) => new FormatPlaceholderV1<T> (false, pt, i, dlm, h, f, ps));
            placeholderTypeRegistry.Add ("C?", (pt, i, dlm, h, f, ps) => new FormatPlaceholderV1<T> (true, pt, i, dlm, h, f, ps));
        }
    }

    public class SimplePlaceholderV1<T> : PlaceholderV1<T>
    {

        public SimplePlaceholderV1 (string parentTemplate, int indexInTemplate, LookupMap<T> dataLookupMap, string headerPart, string formatPart, params string[] parameterNamePart)
            : base(parentTemplate, indexInTemplate, dataLookupMap, headerPart, formatPart, parameterNamePart)
        {
            if (parameterNamePart.Length != 1) {
                throw CreateException (Catalog.GetString ("A simple placeholder requires a single parameter name."));
            }
            SimpleLookup = ParameterLookups [0];
        }

        protected Lookup<T> SimpleLookup { get; private set; }

        public override string ToString (T dataSource)
        {
            object data = SimpleLookup (dataSource);
            return data == null ? "" : data.ToString ();
        }
    }

    public class FormatPlaceholderV1<T> : PlaceholderV1<T>
    {
        public bool IsConditional { get; private set; }

        public FormatPlaceholderV1 (bool conditional, string parentTemplate, int indexInTemplate, LookupMap<T> dataLookupMap, string headerPart, string formatPart, params string[] parameterNamePart)
            : base(parentTemplate, indexInTemplate, dataLookupMap, headerPart, formatPart, parameterNamePart)
        {
            // Test out the format:
            try {
                string.Format (formatPart, parameterNamePart);
            } catch (Exception ex) {
                throw CreateException (Catalog.GetString ("The placeholder has an invalid format."), ex);
            }
            IsConditional = conditional;
        }

        public override string ToString (T dataSource)
        {
            object firstParameter = ParameterLookups [0] (dataSource);
            if (IsConditional && firstParameter == null) {
                return string.Empty;
            }

            // A small optimisation if there is a small number of parameters:
            int len = ParameterLookups.Length;
            if (len == 1) {
                return string.Format (Format, firstParameter);
            } else if (len == 2) {
                return string.Format (Format, firstParameter, ParameterLookups [1] (dataSource));
            } else if (len == 3) {
                return string.Format (Format, firstParameter, ParameterLookups [1] (dataSource), ParameterLookups [2] (dataSource));
            } else {
                // NOTE: We cannot reuse a pre-allocated array as this method may be
                // called from multiple threads.
                object[] values = new object[ParameterLookups.Length];
                for (int i = ParameterLookups.Length - 1; i > 0; --i) {
                    values [i] = ParameterLookups [i] (dataSource);
                }
                values [0] = firstParameter;
                return string.Format (Format, values);
            }
        }
    }

    public class SimpleFormatPlaceholderV1<T> : FormatPlaceholderV1<T>
    {
        public SimpleFormatPlaceholderV1 (bool conditional, string parentTemplate, int indexInTemplate, LookupMap<T> dataLookupMap, string headerPart, string formatPart, params string[] parameterNamePart)
            : base(conditional, parentTemplate, indexInTemplate, dataLookupMap, headerPart, ProcessFormat(formatPart), parameterNamePart)
        {
            if (parameterNamePart == null || parameterNamePart.Length != 1) {
                throw CreateException (Catalog.GetString ("A simple placeholder requires a single parameter name."));
            }
            SimpleLookup = ParameterLookups [0];
        }

        protected Lookup<T> SimpleLookup { get; private set; }

        public override string ToString (T dataSource)
        {
            object data = SimpleLookup (dataSource);
            return (data == null && IsConditional) ? "" : string.Format (Format, data);
        }

        private const char AlignmentFormatDelimiter = ':';

        private static string ProcessFormat (string formatPart)
        {
            if (formatPart.IndexOf (AlignmentFormatDelimiter) < 0) {
                return "{0:" + formatPart + "}";
            } else {
                return "{0," + formatPart + "}";
            }
        }
    }
    #endregion

    #region TemplateV1 Compiler
    internal class TemplateCompilerV1<T>
    {
        #region State
        private string inputTemplate;
        private LookupMap<T> dataLookupMap;
        private List<TemplateSegmentV1<T>> segments;
        private int currentIndex = -1;
        private int templateLength;
        private int state = StateLiteral;
        #region Per-Segment State
        private StringBuilder buffer;
        private string placeholderHeader = string.Empty;
        private string placeholderFormat = string.Empty;
        private List<string> parameterNames;
        private int currentSegmentStartIndex = 0;
        #endregion
        #endregion

        #region Public Interface
        public TemplateCompilerV1 (string template, LookupMap<T> dataLookupMap)
        {
            if (dataLookupMap == null || template == null) {
                throw new TemplateCompilationException (Catalog.GetString ("Non-null template string and data lookup map are required for a V1 template"));
            }
            inputTemplate = template;
            this.dataLookupMap = dataLookupMap;
            buffer = new StringBuilder ();
            parameterNames = new List<string> ();
            segments = new List<TemplateSegmentV1<T>> ();
            templateLength = template.Length;
        }

        public List<TemplateSegmentV1<T>> Segments { get { return segments; } }

        public void Compile ()
        {
            while (NextChar()) {
                switch (state) {
                case StateLiteral:
                    state = processLiteral ();
                    break;
                case StateLiteralEscape:
                    state = processLiteralEscape ();
                    break;
                case StatePlaceholderHeader:
                    state = processPlaceholderHeader ();
                    break;
                case StatePlaceholderHeaderEscape:
                    state = processPlaceholderHeaderEscape ();
                    break;
                case StatePlaceholderFormat:
                    state = processPlaceholderFormat ();
                    break;
                case StatePlaceholderFormatEscape:
                    state = processPlaceholderFormatEscape ();
                    break;
                case StatePlaceholderParameter:
                    state = processPlaceholderParameter ();
                    break;
                case StatePlaceholderParameterEscape:
                    state = processPlaceholderParameterEscape ();
                    break;
                default:
                    throw new TemplateCompilationException (Catalog.GetString ("The Template V1 compiler sufferred an internal error, please report."), inputTemplate, currentIndex);
                }
            }
        }
        #endregion

        #region Finite State Automaton (private)
        private const int StateLiteral = 0;
        private const int StateLiteralEscape = 1;
        private const int StatePlaceholderHeader = 2;
        private const int StatePlaceholderHeaderEscape = 3;
        private const int StatePlaceholderFormat = 4;
        private const int StatePlaceholderFormatEscape = 5;
        private const int StatePlaceholderParameter = 6;
        private const int StatePlaceholderParameterEscape = 7;

        private char CurrentChar { get { return inputTemplate [currentIndex]; } }

        private int CurrentIndex { get { return currentIndex; } }

        private int CurrentSegmentStartIndex { get { return currentSegmentStartIndex; } }

        private bool IsFinished { get { return currentIndex >= templateLength; } }

        private bool NextChar ()
        {
            ++currentIndex;
            return currentIndex < templateLength;
        }

        private void NextSegment ()
        {
            buffer.Clear ();
            currentSegmentStartIndex = currentIndex;
        }

        private void ConsumeChar ()
        {
            buffer.Append (CurrentChar);
        }

        private string ConsumeBuffer ()
        {
            string bufferContent = buffer.ToString ();
            buffer.Clear ();
            return bufferContent;
        }

        private TemplateCompilationException CreateHereException (string message)
        {
            return new TemplateCompilationException (message, inputTemplate, CurrentIndex);
        }

        private void ConsumeSegment (TemplateSegmentV1<T> segment)
        {
            if (segment != null) {
                segments.Add (segment);
            }
            NextSegment ();
        }

        private void ConsumeLiteralSegment ()
        {
            ConsumeSegment (buffer.Length > 0 ? new LiteralSegmentV1<T> (buffer.ToString (), inputTemplate, CurrentSegmentStartIndex) : null);
        }

        private void ConsumeParameter ()
        {
            parameterNames.Add (buffer.ToString ());
            buffer.Clear ();
        }

        private void ConsumePlaceholder ()
        {
            ConsumeSegment (PlaceholderV1<T>.CreatePlaceholder (inputTemplate, CurrentSegmentStartIndex, dataLookupMap, placeholderHeader, placeholderFormat, parameterNames.ToArray ()));
            placeholderFormat = string.Empty;
            placeholderHeader = string.Empty;
            parameterNames.Clear ();
        }

        private int processLiteral ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
                return StateLiteralEscape;
            case PlaceholderV1<T>.PlaceholderStartChar:
                ConsumeLiteralSegment ();
                return StatePlaceholderHeader;
            default:
                ConsumeChar ();
                return StateLiteral;
            }
        }

        private int processLiteralEscape ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
            case PlaceholderV1<T>.PlaceholderStartChar:
                ConsumeChar ();
                return StateLiteral;
            default:
                throw CreateHereException (Catalog.GetString ("An illegal escape sequence."));
            }
        }

        private int processPlaceholderHeader ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
                return StatePlaceholderHeaderEscape;
            case PlaceholderV1<T>.HeaderParametersDelimiter:
                placeholderHeader = ConsumeBuffer ();
                return StatePlaceholderParameter;
            case PlaceholderV1<T>.FormatStartChar:
                placeholderHeader = ConsumeBuffer ();
                return StatePlaceholderFormat;
            case PlaceholderV1<T>.PlaceholderEndChar:
                ConsumeParameter ();
                ConsumePlaceholder ();
                return StateLiteral;
            default:
                ConsumeChar ();
                return StatePlaceholderHeader;
            }
        }

        private int processPlaceholderHeaderEscape ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
            case PlaceholderV1<T>.FormatStartChar:
            case PlaceholderV1<T>.HeaderParametersDelimiter:
                ConsumeChar ();
                return StatePlaceholderHeader;
            default:
                throw CreateHereException (Catalog.GetString ("An illegal escape sequence."));
            }
        }

        private int processPlaceholderFormat ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
                return StatePlaceholderFormatEscape;
            case PlaceholderV1<T>.FormatEndChar:
                placeholderFormat = ConsumeBuffer ();
                return StatePlaceholderParameter;
            case PlaceholderV1<T>.PlaceholderEndChar:
                throw CreateHereException (Catalog.GetString ("Found an incomplete format specification."));
            default:
                ConsumeChar ();
                return StatePlaceholderFormat;
            }
        }

        private int processPlaceholderFormatEscape ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
            case PlaceholderV1<T>.FormatEndChar:
                ConsumeChar ();
                return StatePlaceholderFormat;
            default:
                throw CreateHereException (Catalog.GetString ("An illegal escape sequence."));
            }
        }

        private int processPlaceholderParameter ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
                return StatePlaceholderParameterEscape;
            case PlaceholderV1<T>.ParameterDelimiter:
                ConsumeParameter ();
                return StatePlaceholderParameter;
            case PlaceholderV1<T>.PlaceholderEndChar:
                ConsumeParameter ();
                ConsumePlaceholder ();
                return StateLiteral;
            default:
                ConsumeChar ();
                return StatePlaceholderParameter;
            }
        }

        private int processPlaceholderParameterEscape ()
        {
            switch (CurrentChar) {
            case TemplateV1<T>.EscapeChar:
            case PlaceholderV1<T>.PlaceholderEndChar:
            case PlaceholderV1<T>.ParameterDelimiter:
                ConsumeChar ();
                return StatePlaceholderParameter;
            default:
                throw CreateHereException (Catalog.GetString ("An illegal escape sequence."));
            }
        }
        #endregion
    }
    #endregion
}

