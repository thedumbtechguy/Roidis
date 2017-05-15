using Roidis.Exception;
using Roidis.Service.Converter;
using Roidis.Service.Definition;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

namespace Roidis.Service.Mapper
{
    public class HashMapper : IHashMapper
    {
        private readonly IValueConverter _redisValueConverter;

        public HashMapper(IValueConverter converter)
        {
            _redisValueConverter = converter;
        }

        public List<HashEntry> HashFor<T>(ITypeDefinition<T> definition, T instance, bool isNew, HashEntry[] existingRecord)
        {
            // make a check for indexed fields that are ignored

            var changedList = new List<HashEntry>();

            foreach (var field in definition.AllFields)
            {
                if (isNew && definition.IgnoreOnCreateFields.Any(f => f.Name == field.Name)) continue;
                else if (!isNew && definition.IgnoreOnUpdateFields.Any(f => f.Name == field.Name)) continue;
                
                var value = definition.Accessor[instance, field.Name];
                var redisValue = _redisValueConverter.FromObject(value, field.Type);

                if (definition.RequiredFields.Any(f => f.Name == field.Name) && !redisValue.HasValue)
                    throw new FieldRequiredException(field);

                if (definition.IndexedFields.Contains(field) || redisValue != existingRecord.FirstOrDefault(h => h.Name == field.Name).Value)
                    changedList.Add(new HashEntry(definition.GetHashName(field), redisValue));
            }

            return changedList;
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