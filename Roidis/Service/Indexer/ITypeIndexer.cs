using Roidis.Service.Definition;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Roidis.Service.Indexer
{
    public interface ITypeIndexer
    {
        RedisKey[] GetIndexes<T>(ITypeDefinition<T> definition, List<HashEntry> hashEntries);
    }
}