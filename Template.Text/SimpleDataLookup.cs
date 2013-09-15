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

namespace Template.Text
{
    public class SimpleDataLookup<T>
    {
        private readonly IEnumerable<MemberInfo> members = MemberLookup.GetMembersToBind<T> ();
        private readonly Dictionary<string, Lookup<T>> cachedLookups = new Dictionary<string, Lookup<T>> ();

        /// <summary>
        /// TemplateSharp's user-friendly data lookup map. It looks up information from
        /// data source objects (which are of type `T`) through reflection. It looks up
        /// the classes' (public) members in the following order: any properties with
        /// TemplateParameterAttribute, fields with TPA, methods with TPA, properties
        /// without TPA, fields without TPA, and methods without TPA.
        /// </summary>
        public Lookup<T> CreateValueLookup (string parameter)
        {
            Lookup<T> lookup;
            if (!cachedLookups.TryGetValue (parameter, out lookup)) {
                lookup = MemberLookup.CreateLookupLambdaForMemberExplicitlyBoundToParameter<T> (members, parameter) ??
                    MemberLookup.CreateLookupForMemberWithSameNameAsParameter<T> (members, parameter) ??
                    OnParameterBindingNotFound (parameter);

                cachedLookups.Add (parameter, lookup);
            }
            return lookup;
        }

        public bool IsLookupCached (string parameter)
        {
            return cachedLookups.ContainsKey(parameter);
        }

        protected Lookup<T> OnParameterBindingNotFound (string parameter)
        {
            throw new ArgumentException (string.Format ("Could not bind the parameter '{0}' to any of the members of class '{1}'.", parameter, typeof(T).Name));
        }
    }
}

