using Roidis.Attribute;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class ExpressionParserException : RoidException
    {
        public ExpressionParserException(string message) : base(message) { }
    }
}
