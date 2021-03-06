﻿using FastMember;
using Roidis.Proxy.Object;
using Roidis.Service.Converter;
using Roidis.Service.Definition;
using Roidis.Service.Indexer;
using Roidis.Service.KeyGenerator;
using Roidis.Service.Mapper;
using Roidis.Service.Parser;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;

namespace Roidis
{
    public class Roid : IRoid
    {
        private readonly ConcurrentDictionary<Type, dynamic> _definitionCache;
        private readonly IConnectionMultiplexer _connection;
        private readonly IValueConverter _valueConverter;
        private readonly IStorageKeyGenerator _storageKeyGenerator;
        private readonly IHashMapper _mapper;
        private readonly ITypeIndexer _indexer;
        private readonly ITypeDefinitionFactory _typeDefinitionFactory;
        private readonly IExpressionParser _expressionParser;

        public Roid(IConnectionMultiplexer connection)
        {
            _connection = connection;

            _definitionCache = new ConcurrentDictionary<Type, dynamic>();
            _valueConverter = new ValueConverter();
            _storageKeyGenerator = new StorageKeyGenerator();
            _mapper = new HashMapper(_valueConverter);
            _indexer = new TypeIndexer(_storageKeyGenerator);
            _typeDefinitionFactory = new TypeDefinitionFactory(_valueConverter);
            _expressionParser = new ExpressionParser();
        }

        public IObjectProxy<T> From<T>(int database = 0) where T : new()
        {
            return new ObjectProxy<T>(GetDefinition<T>(), _connection.GetDatabase(database), _valueConverter, _storageKeyGenerator, _mapper, _indexer, _expressionParser);
        }

        private ITypeDefinition<T> GetDefinition<T>()
        {
            var type = typeof(T);

            if (!_definitionCache.ContainsKey(type))
                _definitionCache[type] = _typeDefinitionFactory.Create<T>(TypeAccessor.Create(type));

            return (ITypeDefinition<T>)_definitionCache[type];
        }
    }
}