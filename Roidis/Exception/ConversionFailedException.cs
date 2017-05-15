using StackExchange.Redis;
using System;

namespace Roidis.Exception
{
    public class ConversionFailedException : RoidException
    {
        public ConversionFailedException(RedisValue value, Type type) : base($"Unable to convert '{value}' to '{type.FullName}'")
        {
        }
    }
}