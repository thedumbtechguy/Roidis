using Roidis.Service.Definition;
using Roidis.Service.KeyGenerator;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var indexes = new List<RedisKey>();

            for (var i = 0; i < definition.IndexedFields.Count; i++)
            {
                var indexedField = definition.IndexedFields[i];

                var hashName = definition.GetHashName(indexedField);
                var indexedEntry = hashEntries.FirstOrDefault(h => h.Name == hashName);

                // field removed as it hasn't changed
                if(indexedEntry == default(HashEntry))
                    continue;

                indexes.Add(_storageKeyGenerator.GetFieldIndexKey(definition, indexedField, indexedEntry.Value));
            }

            var indexHashEntry = new HashEntry(Constants.RoidHashFieldIndexes, indexes == null ? string.Empty : string.Join(Constants.Separator, indexes));
            hashEntries.Add(indexHashEntry);

            return indexes.ToArray();
        }
    }
}