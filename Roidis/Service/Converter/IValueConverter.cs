using Roidis.Exception;
using Roidis.Service.Definition;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Roidis.Service.Converter
{
    public interface IValueConverter
    {
        RedisValue FromObject(object sourceValue, Type sourceType);
        object ToObject(RedisValue source, Type targetType);


        bool IsPrimaryKeyTypeSupported(Type type);
        bool IsMemberTypeSupported(Type type);

        Tuple<RedisValue, bool> ResolveId<T>(ITypeDefinition<T> definition, T instance);
    }
}
