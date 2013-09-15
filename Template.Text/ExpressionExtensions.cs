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

    public static class ExpressionExtensions
    {
        public static readonly ConstantExpression Null = Expression.Constant (null);

        public static Expression As<AsType> (this Expression exp)
        {
            return Expression.TypeAs (exp, typeof(AsType));
        }

        public static Expression IfNullThenElse (this Expression exp, Expression ifNull, Expression ifNonNull)
        {
            return Expression.Condition (Expression.Equal (exp, Null), ifNull, ifNonNull, typeof(object));
        }

        public static Expression AssignTo (this Expression exp, Expression to)
        {
            return Expression.Assign (to, exp);
        }

        public static Expression Member (this Expression exp, MemberInfo memberInfo)
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

        public static Expression Member (this Expression exp, string memberName)
        {
            try {
                return Expression.PropertyOrField (exp, memberName);
            } catch (Exception) {
                return Expression.Call (exp, memberName, Type.EmptyTypes);
            }
        }
    }
}
