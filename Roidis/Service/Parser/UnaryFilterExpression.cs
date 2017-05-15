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