using Roidis.Attribute;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Exception
{
    public class ConversionFailedException : RoidException
    {
        public ConversionFailedException(RedisValue value, Type type) : base($"Unable to convert '{value}' to '{type.FullName}'") { }
    }
}
