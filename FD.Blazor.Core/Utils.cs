using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FD.Blazor.Core
{
    public static class Utils
    {

        /// <summary>
        /// Obtains the property name used on an expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(this Expression<Func<T, object>> expression)
        {
            string nameSelector(Expression e)
            {
                switch (e.NodeType)
                {
                    case ExpressionType.Parameter:
                        return ((ParameterExpression)e).Name;

                    case ExpressionType.MemberAccess:
                        return ((MemberExpression)e).Member.Name;

                    case ExpressionType.Call:
                        if (((MethodCallExpression)e).Arguments.Count > 0)
                            return nameSelector(((MethodCallExpression)e).Arguments.FirstOrDefault(m => m.NodeType != ExpressionType.Constant));
                        if (((MethodCallExpression)e).Object is MemberExpression expression1)
                            return expression1.Member.Name;
                        return ((MethodCallExpression)e).Method.Name;

                    case ExpressionType.Conditional:
                        return nameSelector(((MemberExpression)(((ConditionalExpression)e).Test)).Expression);

                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        return nameSelector(((UnaryExpression)e).Operand);

                    case ExpressionType.Invoke:
                        return nameSelector(((InvocationExpression)e).Expression);
                    
                    case ExpressionType.ArrayLength:
                        return "Length";

                    default:
                        throw new NotImplementedException(expression.GetType().ToString());
                }
            }

            return nameSelector(expression.Body);
        }

        /// <summary>
        /// Returns an Expression for a given property name on a specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression<Func<T, object>> GetExpression<T>(string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            // creates a unaryexpression for the property
            Expression conversion = Expression.Convert(Expression.Property(param, propertyName), typeof(object));
            return Expression.Lambda<Func<T, object>>(conversion, param);
        }

        /// <summary>
        /// Creates a delegate for a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Func<T, object> GetFunc<T>(string propertyName)
        {
            return GetExpression<T>(propertyName).Compile();  // only need compiled expression
        }

        /// <summary>
        /// Does creates a BinaryExpression that represents an ExpressionType with the two provided expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> Operation<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, ExpressionType type)
        {
            var invokedExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(type switch {
                ExpressionType.And =>
                    Expression.And(left.Body, invokedExpr),
                ExpressionType.AndAlso =>
                    Expression.AndAlso(left.Body, invokedExpr),
                ExpressionType.Or =>
                    Expression.Or(left.Body, invokedExpr),
                ExpressionType.OrElse =>
                    Expression.OrElse(left.Body, invokedExpr),
                _ =>
                    throw new NotImplementedException(type.GetType().ToString())
            }, 
            left.Parameters);
        }

        /// <summary>
        /// Does an operation between two conditions checking for nulls, in case one of them is null returns the other one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition1"></param>
        /// <param name="condition2"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Operation<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, ExpressionType type, bool ignoreNull = true) =>
            ignoreNull switch
            {
                true =>
                    // if condition1 is null returns condition2, if condition1 is valid but condition2 is null returns condition1, otherwise
                    // does an operation between the 2 expressions
                    left == null ? right : right == null ? left : Utils.Operation<T>(left, right, type),
                false =>
                    Utils.Operation<T>(left, right, type)
            };

        /// <summary>
        /// Determinates if a method runs asynchronously.
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static bool IsAsyncMethod(Type classType, string methodName)
        {
            // Obtain the method with the specified name.
            MethodInfo method = classType.GetMethod(methodName);
            return IsAsyncMethod(method);
        }

        /// <summary>
        /// Determines if a method runs asynchronously.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsAsyncMethod(MethodInfo method)
        {
            Type attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method.GetCustomAttribute(attType);

            return (attrib != null);
        }
    }
}
