using Roidis.Service.Definition;
using StackExchange.Redis;
using System;

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