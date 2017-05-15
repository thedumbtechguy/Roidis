using System;

namespace Roidis.Service.Parser
{
    public struct Filter
    {
        public string OperandName { get; set; }

        public FilterOperator Operator { get; set; }

        public Type RightType { get; set; }
        public object RightValue { get; set; }

        override public string ToString()
        {
            return $"{OperandName} {Operator} ({RightType}) {RightValue}";
        }
    }

    public enum FilterOperator
    {
        Equals,
        NotEquals
    }
}