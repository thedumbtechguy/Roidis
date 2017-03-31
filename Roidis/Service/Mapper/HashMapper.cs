using Roidis.Service.Converter;
using Roidis.Service.Definition;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roidis.Service.Mapper
{
    public class HashMapper : IHashMapper
    {
        private readonly IValueConverter _redisValueConverter;

        public HashMapper(IValueConverter converter)
        {
            _redisValueConverter = converter;
        }

        public List<HashEntry> HashFor<T>(ITypeDefinition<T> definition, T instance) where T : new()
        {
            var list = new List<HashEntry>();

            foreach (var field in definition.AllFields)
            {
                var value = definition.Accessor[instance, field.Name];
                var redisValue = _redisValueConverter.FromObject(value, field.Type);

                list.Add(new HashEntry(definition.GetHashName(field), redisValue));
            }

            return list;
        }

        public T InstanceFor<T>(ITypeDefinition<T> definition, HashEntry[] hash) where T : new()
        {
            var obj = new T();

            foreach (var field in definition.AllFields)
            {
                var hashValue = hash.FirstOrDefault(h => h.Name == definition.GetHashName(field)).Value;
                var properValue = _redisValueConverter.ToObject(hashValue, field.Type);
                definition.Accessor[obj, field.Name] = properValue;
            }

            return obj;
        }
    }
}
