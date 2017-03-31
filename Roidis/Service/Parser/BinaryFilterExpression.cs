using Roidis.Exception;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Roidis.Service.Parser
{
    public struct BinaryFilterExpression: IFilterExpression
    {
        public Filter Left { get; set; }
        public Filter Right { get; set; }
        public ComparisonOperator Comparison { get; set; }

        override public string ToString()
        {
            return $"BinaryFilter: {Left} {Comparison} {Right}";
        }
    }
}
