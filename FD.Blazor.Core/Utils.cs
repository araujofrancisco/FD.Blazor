using System;
using System.Linq;
using System.Linq.Expressions;

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
        public static string GetPropertyName<T>(this Expression<Func<T, object>> expression) =>
            expression.Body switch
            {
                MemberExpression m =>
                    m.Member.Name,
                UnaryExpression u when u.Operand is MemberExpression m =>
                    m.Member.Name,
                MethodCallExpression c when c.Arguments.Count > 0 =>
                    ((MemberExpression)c.Arguments[0]).Member.Name,
                MethodCallExpression o when o.Object is MemberExpression m =>
                    m.Member.Name,
                ConditionalExpression i when i.Test is MemberExpression m =>
                                    ((MemberExpression)m.Expression).Member.Name,
                _ =>
                    throw new NotImplementedException(expression.GetType().ToString())
            };

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
        /// Does creates a BinaryExpression that represents an AndAlso with the two provided expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var invokedExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
        }
    }
}
