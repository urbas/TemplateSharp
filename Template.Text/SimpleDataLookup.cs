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
using System.Linq.Expressions;

namespace Template.Text
{
    public static class SimpleDataLookup
    {
        /// <summary>
        /// TemplateSharp's user-friendly data lookup map. It looks up information from
        /// data source objects (which are of type `T`) through reflection. It looks up
        /// the classes' (public) members in the following order: any properties with
        /// TemplateParameterAttribute, fields with TPA, methods with TPA, properties
        /// without TPA, fields without TPA, and methods without TPA.
        /// </summary>
        public static Lookup<T> SimpleLookupMap<T> (string parameter)
        {
            // This method uses reflection to map the `parameter` to (parameterless) members
            // of `T`.
            Type clazz = typeof(T);

            // Find the first property which maps to the parameter name:
            var props = clazz.GetProperties (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (props != null && props.Length > 0) {
                foreach (var prop in props) {
                    if (prop.CanRead) {
                        // Find all properties that have the TP attribute with a name binding:
                        var attrs = prop.GetCustomAttributes (typeof(TemplateParameter), true);
                        if (attrs != null && attrs.Length > 0) {
                            foreach (TemplateParameter tpa in attrs) {
                                // Is this property bound to the sought parameter?
                                if (tpa.Name == null ? string.Equals(prop.Name, parameter) : string.Equals (tpa.Name, parameter)) {
                                    // Yes, it is! Just return it's lookup function:
                                    // Is it a direct value?
                                    if (tpa.Property == null) {
                                        // Yes, just return the value of the property:
                                        var param = Expression.Parameter (typeof(T), "dataSourceObject");
                                        return Expression.Lambda<Lookup<T>> (param.Property (prop).As<object> (), param).Compile ();
                                    } else {
                                        // Nope, we have to return the submember:
                                        /* Returns this:
                                        return dso => {
                                            var tmp;
                                            if (dso == null)
                                              return null;
                                            tmp = dso.Property;
                                            if (tmp == null)
                                              return null;
                                            return tmp.SubMember;
                                        }
                                        */
                                        var param = Expression.Parameter (typeof(T), "dso");
                                        var tmpVariable = Expression.Variable (prop.PropertyType, "tmpVariable");
                                        var propertyValue = param.Property (prop).AssignTo (tmpVariable);
                                        var getBlock = Expression.Block (
                                            typeof(object),
                                            new ParameterExpression[]{ tmpVariable },
                                            param.IfNotNull (propertyValue.IfNotNull (tmpVariable.Member (tpa.Property).As<object> ())));
                                        return Expression.Lambda<Lookup<T>> (getBlock, param).Compile ();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var fields = clazz.GetFields (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (fields != null && fields.Length > 0) {
                foreach (var field in fields) {
                    // Find all fields that have the TP attribute with a name binding:
                    var attrs = field.GetCustomAttributes (typeof(TemplateParameter), true);
                    if (attrs != null && attrs.Length > 0) {
                        foreach (TemplateParameter tpa in attrs) {
                            // Is this field bound to the sought parameter?
                            if (tpa.Name == null ? string.Equals(field.Name, parameter) : string.Equals (tpa.Name, parameter)) {
                                // Yes, it is! Just return it's lookup function:
                                // Is it a direct value?
                                if (tpa.Property == null) {
                                    // Yes, just return the value of the field:
                                    var param = Expression.Parameter(typeof(T), "dataSourceObject");
                                    return Expression.Lambda<Lookup<T>>(param.Field(field).As<object>(), param).Compile();
                                } else {
                                    // Nope, we have to return the submember:
                                    var param = Expression.Parameter(typeof(T), "dso");
                                    var tmpVariable = Expression.Variable(field.FieldType, "tmpVariable");
                                    var propertyValue = param.Field(field).AssignTo(tmpVariable);
                                    var getBlock = Expression.Block(
                                        typeof(object),
                                        new ParameterExpression[]{ tmpVariable },
                                        param.IfNotNull(propertyValue.IfNotNull(tmpVariable.Member(tpa.Property).As<object>())));
                                    return Expression.Lambda<Lookup<T>>(getBlock, param).Compile();
                                }
                            }
                        }
                    }
                }
            }

            var methods = clazz.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (methods != null && methods.Length > 0) {
                foreach (var method in methods) {
                    // Find all methods that have the TP attribute with a name binding:
                    var attrs = method.GetCustomAttributes (typeof(TemplateParameter), true);
                    if (attrs != null && attrs.Length > 0) {
                        foreach (TemplateParameter tpa in attrs) {
                            // Is this method bound to the sought parameter?
                            if (tpa.Name == null ? string.Equals(method.Name, parameter) : string.Equals (tpa.Name, parameter)) {
                                // Yes, it is! Just return it's lookup function:
                                // Is it a direct value?
                                if (tpa.Property == null) {
                                    // Yes, just return the value of the method:
                                    var param = Expression.Parameter(typeof(T), "dataSourceObject");
                                    return Expression.Lambda<Lookup<T>>(method.InvokeOn(param).As<object>(), param).Compile();
                                } else {
                                    // Nope, we have to return the submember:
                                    var param = Expression.Parameter(typeof(T), "dso");
                                    var tmpVariable = Expression.Variable(method.ReturnType, "tmpVariable");
                                    var callValue = method.InvokeOn(param).AssignTo(tmpVariable);
                                    var getBlock = Expression.Block(
                                        typeof(object),
                                        new ParameterExpression[]{ tmpVariable },
                                        param.IfNotNull(callValue.IfNotNull(tmpVariable.Member(tpa.Property).As<object>())));
                                    return Expression.Lambda<Lookup<T>>(getBlock, param).Compile();
                                }
                            }
                        }
                    }
                }
            }

            // Now find direct bindings (properties, fields and methods that bind to parameters without the help of TP attributes):
            if (props != null && props.Length > 0) {
                foreach (var prop in props) {
                    if (prop.CanRead && string.Equals(parameter, prop.Name)) {
                        var param = Expression.Parameter (typeof(T), "dataSourceObject");
                        return Expression.Lambda<Lookup<T>> (param.Property (prop).As<object> (), param).Compile ();
                    }
                }
            }
            if (fields != null && fields.Length > 0) {
                foreach (var field in fields) {
                    if (string.Equals(parameter, field.Name)) {
                        var param = Expression.Parameter (typeof(T), "dataSourceObject");
                        return Expression.Lambda<Lookup<T>> (param.Field (field).As<object> (), param).Compile ();
                    }
                }
            }
            if (methods != null && methods.Length > 0) {
                foreach (var method in methods) {
                    if (string.Equals(parameter, method.Name)) {
                        var param = Expression.Parameter (typeof(T), "dataSourceObject");
                        return Expression.Lambda<Lookup<T>> (method.InvokeOn(param).As<object> (), param).Compile ();
                    }
                }
            }
            return null;
        }

        #region Helper Expression Methods (static, private)
        private static Expression EqualsNull (this Expression exp)
        {
            return Expression.Equal (exp, Null ());
        }

        private static ConstantExpression Null ()
        {
            return Expression.Constant (null);
        }

        private static Expression As<AsType> (this Expression exp)
        {
            return Expression.TypeAs (exp, typeof(AsType));
        }

        private static Expression PropertyOrField (this Expression exp, string propertyOrFieldName)
        {
            return Expression.PropertyOrField (exp, propertyOrFieldName);
        }

        private static Expression Property (this Expression exp, PropertyInfo propInfo)
        {
            return Expression.Property (exp, propInfo);
        }

        private static Expression IfNullThenElse<T> (this Expression exp, Expression ifNull, Expression ifNonNull)
        {
            return Expression.Condition (exp.EqualsNull (), ifNull, ifNonNull, typeof(T));
        }

        private static Expression IfNullThenElse (this Expression exp, Expression ifNull, Expression ifNonNull)
        {
            return Expression.Condition (exp.EqualsNull (), ifNull, ifNonNull, typeof(object));
        }

        private static Expression AssignTo (this Expression exp, Expression to)
        {
            return Expression.Assign (to, exp);
        }

        private static Expression IfNotNull (this Expression exp, Expression ifNotNull)
        {
            return exp.IfNullThenElse (Null (), ifNotNull);
        }

        private static Expression IfNotNull<T> (this Expression exp, Expression ifNotNull)
        {
            return exp.IfNullThenElse<T> (Null (), ifNotNull);
        }

        private static Expression Field (this Expression exp, FieldInfo fieldInfo)
        {
            return Expression.Field (exp, fieldInfo);
        }

        private static Expression InvokeOn(this MethodInfo method, Expression instance)
        {
            return Expression.Call(instance, method);
        }

        private static Expression Member(this Expression exp, string propertyOrFieldOrMethod)
        {
            try
            {
                return exp.PropertyOrField(propertyOrFieldOrMethod);
            }
            catch (ArgumentException)
            {
                return Expression.Call(exp, propertyOrFieldOrMethod, null);
            }
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
    public class TemplateParameter : Attribute
    {
        public string Name;
        public string Property;

        public TemplateParameter ()
        {
        }

        public TemplateParameter (string name)
        {
            this.Name = name;
        }
    }
}

