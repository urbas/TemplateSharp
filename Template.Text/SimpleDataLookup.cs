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
            var props = FindReadableProperties<T> ();
            var methods = FindParameterlessFunctions<T> ();
            var fields = typeof(T).GetFields (BindingFlagsOfMembersToInspect);

            var allMembers = props.Concat (methods).Concat (fields);

            var lookup = CreateLookupLambdaForMemberExplicitlyBoundToParameter<T> (allMembers, parameter);
            if (lookup != null)
                return lookup;

            foreach (var prop in allMembers) {
                if (string.Equals (parameter, prop.Name)) {
                    return CreateDirectMemberLookupLambda<T> (prop);
                }
            }

            return null;
        }

        private const BindingFlags BindingFlagsOfMembersToInspect = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static IEnumerable<MemberInfo> FindReadableProperties<T> ()
        {
            var props = typeof(T).GetProperties (BindingFlagsOfMembersToInspect);
            
            if (props != null && props.Length > 0) {
                return (from prop in props where prop.CanRead select prop).ToArray ();
            } else {
                return new PropertyInfo[]{};
            }
        }

        public static IEnumerable<MemberInfo> FindParameterlessFunctions<T> ()
        {
            var methods = typeof(T).GetMethods (BindingFlagsOfMembersToInspect);
            
            if (methods != null && methods.Length > 0) {
                return (from method
                        in methods
                        where method.GetParameters ().Length == 0 && !method.Name.StartsWith ("get_") && method.ReturnType != typeof(void)
                        select method).ToArray ();
            } else {
                return new MemberInfo[]{};
            }
        }

        public static Tuple<MemberInfo, TemplateParameter> FindMemberExplicitlyBoundToParameter (IEnumerable<MemberInfo> classMembers, string parameter)
        {
            foreach (var memberInfo in classMembers) {
                var templateParameterAttributes = memberInfo.GetCustomAttributes (typeof(TemplateParameter), true);

                foreach (TemplateParameter templateParameterAttribute in templateParameterAttributes) {
                    if (IsMemberExplicitlyBoundToParameter (memberInfo, templateParameterAttribute, parameter)) {
                        return new Tuple<MemberInfo, TemplateParameter> (memberInfo, templateParameterAttribute);
                    }
                }
            }

            return null;
        }

        public static bool IsMemberExplicitlyBoundToParameter (MemberInfo member, TemplateParameter parameterBindingAttribute, string parameter)
        {
            return string.Equals (parameterBindingAttribute.Name, parameter) || (parameterBindingAttribute.Name == null && string.Equals (member.Name, parameter));
        }

        public static Lookup<T> CreateLookupLambdaForMemberExplicitlyBoundToParameter<T> (IEnumerable<MemberInfo> props, string parameter)
        {
            var memberInfoWithAttribute = FindMemberExplicitlyBoundToParameter (props, parameter);
            if (memberInfoWithAttribute != null) {
                var propertyBoundToParameter = memberInfoWithAttribute.Item1;
                var templateParameterAttribute = memberInfoWithAttribute.Item2;
                return CreateParameterLookupLambda<T> (templateParameterAttribute, propertyBoundToParameter);
            }
            return null;
        }

        public static Lookup<T> CreateParameterLookupLambda<T> (TemplateParameter parameterBindingInfo, MemberInfo memberBoundToParameter)
        {
            if (parameterBindingInfo.Property == null) {
                return CreateDirectMemberLookupLambda<T> (memberBoundToParameter);
            } else {
                return ConstructSubmemberAccessLambda<T> (memberBoundToParameter, parameterBindingInfo.Property);
            }
        }

        public static Lookup<T> CreateDirectMemberLookupLambda<T> (MemberInfo propertyBoundToParameter)
        {
            var dataSourceObject = Expression.Parameter (typeof(T), "dataSourceObject");
            return Expression.Lambda<Lookup<T>> (dataSourceObject.Member (propertyBoundToParameter).As<object> (), dataSourceObject).Compile ();
        }

        public static Lookup<T> ConstructSubmemberAccessLambda<T> (MemberInfo memberBoundToParameter, string submemberName)
        {
            /* Returns this:
            return dataSourceObject => {
                var tmp;
                if (dataSourceObject != null) {
                    tmp = dataSourceObject.Member;
                    if (tmp != null) {
                        return tmp.SubMember;
                    }
                }
                return null;
            }
            */
            var dataSourceObject = Expression.Parameter (typeof(T), "dso");

            var returnTypeOfMember = GetReturnTypeOf (memberBoundToParameter);

            var memberReturnValue = Expression.Variable (returnTypeOfMember, "memberReturnValue");
            var submemberValue = memberReturnValue.Member (submemberName).As<object> ();
            var assignMemberReturnValue = dataSourceObject.Member (memberBoundToParameter).AssignTo (memberReturnValue);

            var ifMemberNull = assignMemberReturnValue.IfNullThenElse (Null, submemberValue);
            var ifDataSourceObjectNull = dataSourceObject.IfNullThenElse (Null, ifMemberNull);

            var getBlock = Expression.Block (typeof(object), new ParameterExpression[]{ memberReturnValue }, ifDataSourceObjectNull);
            return Expression.Lambda<Lookup<T>> (getBlock, dataSourceObject).Compile ();
        }

        private static Expression EqualsNull (this Expression exp)
        {
            return Expression.Equal (exp, Null);
        }

        private static readonly ConstantExpression Null = Expression.Constant (null);

        private static Expression As<AsType> (this Expression exp)
        {
            return Expression.TypeAs (exp, typeof(AsType));
        }

        private static Expression IfNullThenElse (this Expression exp, Expression ifNull, Expression ifNonNull)
        {
            return Expression.Condition (exp.EqualsNull (), ifNull, ifNonNull, typeof(object));
        }

        private static Expression AssignTo (this Expression exp, Expression to)
        {
            return Expression.Assign (to, exp);
        }

        private static Expression Member (this Expression exp, MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
                return Expression.Property (exp, (PropertyInfo)memberInfo);
            else if (memberInfo is FieldInfo)
                return Expression.Field (exp, (FieldInfo)memberInfo);
            else if (memberInfo is MethodInfo)
                return Expression.Call (exp, (MethodInfo)memberInfo);
            else
                throw new ArgumentException ("Unknown member.", "memberInfo");
        }

        private static Expression Member (this Expression exp, string memberName)
        {
            try {
                return Expression.PropertyOrField (exp, memberName);
            } catch (Exception) {
                return Expression.Call (exp, memberName, Type.EmptyTypes);
            }
        }

        private static Type GetReturnTypeOf (MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).PropertyType;
            else if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).FieldType;
            else if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).ReturnType;
            else
                throw new ArgumentException ("Unknown member.", "memberInfo");
        }
    }
}

