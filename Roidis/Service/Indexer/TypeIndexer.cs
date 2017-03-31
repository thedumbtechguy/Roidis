using Roidis.Service.Converter;
using Roidis.Service.Definition;
using Roidis.Service.KeyGenerator;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roidis.Service.Indexer
{
    public class TypeIndexer : ITypeIndexer
    {
        private readonly IStorageKeyGenerator _storageKeyGenerator;

        public TypeIndexer(IStorageKeyGenerator keyGenerator)
        {
            _storageKeyGenerator = keyGenerator;
        }

        public RedisKey[] GetIndexes<T>(ITypeDefinition<T> definition, List<HashEntry> hashEntries)
        {
            var indexes = new RedisKey[definition.IndexedFields.Count];

            for (var i = 0; i < definition.IndexedFields.Count; i++)
            {
                var indexedField = definition.IndexedFields[i];

                var hashName = definition.GetHashName(indexedField);
                var indexedEntry = hashEntries.First(h => h.Name == hashName);

                indexes[i] = _storageKeyGenerator.GetFieldIndexKey(definition, indexedField, indexedEntry.Value);
            }

            var indexHashEntry = new HashEntry(Constants.RoidHashFieldIndexes, indexes == null ? string.Empty : string.Join(Constants.Separator, indexes));
            hashEntries.Add(indexHashEntry);

            return indexes;
        }
    }
}
