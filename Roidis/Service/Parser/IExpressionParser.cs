using System;
using System.Linq.Expressions;

namespace Roidis.Service.Parser
{
    public interface IExpressionParser
    {
        IFilterExpression ParseFilter<T>(Expression<Func<T, bool>> expression);

        string GetPropertyName(Expression expr);
    }
}