using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Roidis.Service.Parser
{
    public interface  IExpressionParser
    {
        IFilterExpression ParseFilter<T>(Expression<Func<T, bool>> expression);
        string GetPropertyName(Expression expr);
    }
}
