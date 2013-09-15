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

    public static class MemberLookup
    {

        public static Lookup<T> CreateLookupForMemberWithSameNameAsParameter<T> (IEnumerable<MemberInfo> members, string parameter)
        {
            foreach (var prop in members) {
                if (string.Equals (parameter, prop.Name)) {
                    return MemberLookup.CreateDirectMemberLookupLambda<T> (prop);
                }
            }
            return null;
        }

        public static Lookup<T> CreateLookupLambdaForMemberExplicitlyBoundToParameter<T> (IEnumerable<MemberInfo> members, string parameter)
        {
            var memberInfoWithAttribute = MemberLookup.FindMemberExplicitlyBoundToParameter (members, parameter);
            if (memberInfoWithAttribute != null) {
                var propertyBoundToParameter = memberInfoWithAttribute.Item1;
                var templateParameterAttribute = memberInfoWithAttribute.Item2;
                return MemberLookup.CreateParameterLookupLambda<T> (templateParameterAttribute, propertyBoundToParameter);
            }
            return null;
        }

        public static Tuple<MemberInfo, TemplateParameter> FindMemberExplicitlyBoundToParameter (IEnumerable<MemberInfo> members, string parameter)
        {
            foreach (var memberInfo in members) {
                var templateParameterAttributes = memberInfo.GetCustomAttributes (typeof(TemplateParameter), true);

                foreach (TemplateParameter templateParameterAttribute in templateParameterAttributes) {
                    if (MemberLookup.IsMemberExplicitlyBoundToParameter (memberInfo, templateParameterAttribute, parameter)) {
                        return new Tuple<MemberInfo, TemplateParameter> (memberInfo, templateParameterAttribute);
                    }
                }
            }

            return null;
        }

        public static IEnumerable<MemberInfo> GetMembersToBind<TDataSource> ()
        {
            var props = FindReadableProperties<TDataSource> ();
            var methods = FindParameterlessFunctions<TDataSource> ();
            var fields = typeof(TDataSource).GetFields (BindingFlagsOfMembersToInspect);

            return props.Concat (methods).Concat (fields);
        }
        
        private const BindingFlags BindingFlagsOfMembersToInspect = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static IEnumerable<MemberInfo> FindReadableProperties<TDataSource> ()
        {
            var props = typeof(TDataSource).GetProperties (BindingFlagsOfMembersToInspect);
            
            if (props != null && props.Length > 0) {
                return (from prop in props where prop.CanRead select prop).ToArray ();
            } else {
                return new PropertyInfo[]{};
            }
        }

        public static IEnumerable<MemberInfo> FindParameterlessFunctions<TDataSource> ()
        {
            var methods = typeof(TDataSource).GetMethods (BindingFlagsOfMembersToInspect);
            
            if (methods != null && methods.Length > 0) {
                return (from method
                        in methods
                        where method.GetParameters ().Length == 0 && !method.Name.StartsWith ("get_") && method.ReturnType != typeof(void)
                        select method).ToArray ();
            } else {
                return new MemberInfo[]{};
            }
        }

        public static bool IsMemberExplicitlyBoundToParameter (MemberInfo member, TemplateParameter parameterBindingAttribute, string parameter)
        {
            return string.Equals (parameterBindingAttribute.Name, parameter) || (parameterBindingAttribute.Name == null && string.Equals (member.Name, parameter));
        }

        public static Lookup<TDataSource> CreateParameterLookupLambda<TDataSource> (TemplateParameter parameterBindingInfo, MemberInfo memberBoundToParameter)
        {
            if (parameterBindingInfo.Property == null) {
                return CreateDirectMemberLookupLambda<TDataSource> (memberBoundToParameter);
            } else {
                return ConstructSubmemberAccessLambda<TDataSource> (memberBoundToParameter, parameterBindingInfo.Property);
            }
        }

        public static Lookup<TDataSource> CreateDirectMemberLookupLambda<TDataSource> (MemberInfo propertyBoundToParameter)
        {
            var dataSourceObject = Expression.Parameter (typeof(TDataSource), "dataSourceObject");
            return Expression.Lambda<Lookup<TDataSource>> (dataSourceObject.Member (propertyBoundToParameter).As<object> (), dataSourceObject).Compile ();
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

            var ifMemberNull = assignMemberReturnValue.IfNullThenElse (ExpressionExtensions.Null, submemberValue);
            var ifDataSourceObjectNull = dataSourceObject.IfNullThenElse (ExpressionExtensions.Null, ifMemberNull);

            var getBlock = Expression.Block (typeof(object), new ParameterExpression[]{ memberReturnValue }, ifDataSourceObjectNull);
            return Expression.Lambda<Lookup<T>> (getBlock, dataSourceObject).Compile ();
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
