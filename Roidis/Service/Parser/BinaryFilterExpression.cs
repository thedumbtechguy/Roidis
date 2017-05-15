namespace Roidis.Service.Parser
{
    public struct BinaryFilterExpression : IFilterExpression
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