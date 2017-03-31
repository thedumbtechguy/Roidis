using Roidis.Exception;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Roidis.Service.Parser
{
    public struct UnaryFilterExpression : IFilterExpression
    {
        public Filter Filter { get; set; }

        override public string ToString()
        {
            return $"Filter: {Filter}";
        }
    }
}
