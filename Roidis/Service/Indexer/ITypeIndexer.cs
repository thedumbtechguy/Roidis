using Roidis.Service.Converter;
using Roidis.Service.Definition;
using Roidis.Service.KeyGenerator;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roidis.Service.Indexer
{
    public interface ITypeIndexer
    {
        RedisKey[] GetIndexes<T>(ITypeDefinition<T> definition, List<HashEntry> hashEntries);
    }
}
