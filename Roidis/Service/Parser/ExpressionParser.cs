using Roidis.Exception;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Roidis.Service.Parser
{
    public class ExpressionParser : IExpressionParser
    {
        public IFilterExpression ParseFilter<T>(Expression<Func<T, bool>> expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                return ParseFilterExpression(lambdaExpression.Body);
            }

            throw new ArgumentException($"'{expression}' is not a valid LambdaExpression.");
        }

        private IFilterExpression ParseFilterExpression(Expression expression, int binaryDepth = 0)
        {
            if (binaryDepth > 1) throw new ExpressionParserException("You can only combine two expressions in a filter.");

            if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.Equal || binaryExpression.NodeType == ExpressionType.NotEqual)
                {
                    return new UnaryFilterExpression()
                    {
                        Filter = ParseComparison(binaryExpression)
                    };
                }
                else if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    binaryDepth++;

                    return new BinaryFilterExpression()
                    {
                        Left = ((UnaryFilterExpression)ParseFilterExpression(binaryExpression.Left, binaryDepth)).Filter,

                        Comparison = binaryExpression.NodeType == ExpressionType.AndAlso ? ComparisonOperator.And : ComparisonOperator.Or,

                        Right = ((UnaryFilterExpression)ParseFilterExpression(binaryExpression.Right, binaryDepth)).Filter,
                    };
                }
            }
            else if (expression is MemberExpression memberExpression)
            {
                return new UnaryFilterExpression()
                {
                    Filter = ParsePropertyFilter(memberExpression)
                };
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                return new UnaryFilterExpression()
                {
                    Filter = ParseUnaryFilter(unaryExpression)
                };
            }

            throw new ExpressionParserException($"Expression not supported '{expression}'");
        }

        private Filter ParseUnaryFilter(UnaryExpression unaryExpression)
        {
            return new Filter()
            {
                OperandName = GetPropertyName(unaryExpression),

                Operator = FilterOperator.NotEquals,

                RightType = unaryExpression.Type,
                RightValue = true,
            };
        }

        private Filter ParsePropertyFilter(MemberExpression memberExpression)
        {
            return new Filter()
            {
                OperandName = GetPropertyName(memberExpression),

                Operator = FilterOperator.Equals,

                RightType = memberExpression.Type,
                RightValue = true,
            };
        }

        private Filter ParseComparison(BinaryExpression comparison)
        {
            return new Filter()
            {
                OperandName = GetPropertyName(comparison.Left),

                Operator = comparison.NodeType == ExpressionType.Equal ? FilterOperator.Equals : FilterOperator.NotEquals,

                RightType = comparison.Right.Type,
                RightValue = Expression.Lambda(comparison.Right).Compile().DynamicInvoke(),
            };
        }

        private void Parse(BinaryExpression comparison)
        {
            var rightType = comparison.Right.Type;
            var rightVal = Expression.Lambda(comparison.Right).Compile().DynamicInvoke();

            var comparisonType = comparison.NodeType;

            var leftVal = GetPropertyName(comparison.Left);
            Console.WriteLine($"{leftVal} {comparisonType} ({rightType}) {rightVal}");
        }

        private MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression;
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                return ((MemberExpression)unaryExpression.Operand);
            }
            else if (expression is LambdaExpression lambdaExpression)
            {
                return GetMemberExpression(lambdaExpression.Body);
            }

            throw new ExpressionParserException($"'{expression.GetType().GetTypeInfo().GetEnumUnderlyingType()}' cannot be converted to a MemberExpression.");
        }

        public string GetPropertyName(Expression expr)
        {
            return GetMemberExpression(expr).Member.Name;
        }
    }
}