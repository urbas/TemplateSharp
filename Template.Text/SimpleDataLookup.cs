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

namespace Template.Text
{
    public static class SimpleDataLookup
    {
        public static Lookup<T> SimpleLookupMap<T>(string parameter)
        {
            // This method uses reflection to map the `parameter` to (parameterless) members
            // of `T`.
            Type clazz = typeof(T);

            // First get all (both instance and static) public get properties, public parameterless methods, and public fields (in this order):
            var props = clazz.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (props != null && props.Length > 0) {
                foreach (var prop in props) {
                    if (prop.CanRead) {
                        var attrs = prop.GetCustomAttributes(typeof(TemplateParameter), true);
                        if (attrs == null || attrs.Length < 1) {
                            // TODO: Just check the name.
                        } else {
                            // TODO: Check all its attributes.
                        }
                    }
                }
            }

            //var methods = clazz.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            //var fields = clazz.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class TemplateParameter : Attribute
    {
        public string name;
        public string property;

        public TemplateParameter ()
        {
        }

        public TemplateParameter(string name)
        {
            this.name = name;
        }
    }
}

