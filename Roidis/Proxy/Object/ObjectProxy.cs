using Roidis.Exception;
using Roidis.Service.Converter;
using Roidis.Service.Definition;
using Roidis.Service.Indexer;
using Roidis.Service.KeyGenerator;
using Roidis.Service.Mapper;
using Roidis.Service.Parser;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Roidis.Proxy.Object
{
    sealed public class ObjectProxy<T> : IObjectProxy<T> where T : new()
    {
        private readonly IDatabase _database;
        private readonly IValueConverter _valueConverter;
        private readonly IStorageKeyGenerator _storageKeyGenerator;
        private readonly IHashMapper _mapper;
        private readonly ITypeIndexer _indexer;
        private readonly ITypeDefinition<T> _typeDefinition;
        private readonly IExpressionParser _parser;

        public ObjectProxy(ITypeDefinition<T> typeDefinition, IDatabase database, IValueConverter valueConverter, IStorageKeyGenerator storageKeyGenerator, IHashMapper hashMapper, ITypeIndexer typeIndexer, IExpressionParser parser)
        {
            _database = database;

            _valueConverter = valueConverter;
            _storageKeyGenerator = storageKeyGenerator;
            _mapper = hashMapper;
            _indexer = typeIndexer;
            _typeDefinition = typeDefinition;
            _parser = parser;
        }

        public async Task<T> Save(T instance, bool saveChangedOnly = true)
        {
            var serialKey = _storageKeyGenerator.GetSerialKey(_typeDefinition);
            var clusterIndexKey = _storageKeyGenerator.GetClusteredIndexKey(_typeDefinition);

            var _val = _valueConverter.ResolveId(_typeDefinition, instance);
            RedisValue id = _val.Item1;
            bool wasIdSet = _val.Item2;

            RedisKey key;
            var existingIndexes = new RedisKey[0];
            var existingRecord = new HashEntry[0];
            long serial = -1;
            if (!wasIdSet)
            {
                // this is a new object so we need to add it to the clustered index...
                serial = await _database.StringIncrementAsync(serialKey).ConfigureAwait(false);

                // guid ids are auto generated and not null
                if (id.IsNull) id = serial;

                // and set the id on the object
                _typeDefinition.Accessor[instance, _typeDefinition.PrimaryKey.Name] = _valueConverter.ToObject(id, _typeDefinition.PrimaryKey.Type);
                key = _storageKeyGenerator.GetKey(_typeDefinition, id);
            }
            else
            {
                string rawIndexes = null;
                key = _storageKeyGenerator.GetKey(_typeDefinition, id);

                if (saveChangedOnly)
                {
                    existingRecord = await _database.HashGetAllAsync(key).ConfigureAwait(false);
                    rawIndexes = existingRecord.FirstOrDefault(h => h.Name == Constants.RoidHashFieldIndexes).Value;
                }
                else
                {
                    rawIndexes = await _database.HashGetAsync(key, Constants.RoidHashFieldIndexes).ConfigureAwait(false);
                }

                // if rawIndexes is null, that means we have not yet saved the object
                // id is user provided in this case hence we need to add it to the clustered index
                if (string.IsNullOrWhiteSpace(rawIndexes))
                {
                    serial = await _database.StringIncrementAsync(serialKey).ConfigureAwait(false);
                }
                else
                {
                    existingIndexes = rawIndexes.Split(new[] { Constants.Separator }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(i => (RedisKey)i)
                                        .ToArray();
                }
            }

            bool isNew = serial != -1;
            var hash = _mapper.HashFor(_typeDefinition, instance, isNew, existingRecord);

            var indexes = _indexer.GetIndexes(_typeDefinition, hash);
            var indexesToRemove = existingIndexes.Where(i => !indexes.Contains(i)).ToArray();
            var indexesToAdd = indexes.Where(i => !existingIndexes.Contains(i)).ToArray();

            #region save to redis

            var keyString = (string)key;
            var txn = _database.CreateTransaction();

#pragma warning disable CS4014

            txn.HashSetAsync(key, hash.ToArray());

            if (isNew)
                txn.SortedSetAddAsync(clusterIndexKey, new[] { new SortedSetEntry(keyString, serial) });

            foreach (var index in indexesToAdd) txn.SetAddAsync(index, keyString);
            foreach (var index in indexesToRemove) txn.SetRemoveAsync(index, keyString);

#pragma warning restore CS4014

            if (!await txn.ExecuteAsync().ConfigureAwait(false))
            {
                throw new RoidException("Failed to commit the transaction");
            }

            #endregion save to redis

            return instance;
        }

        public async Task<T> Fetch(RedisValue id)
        {
            var key = _storageKeyGenerator.GetKey(_typeDefinition, id);

            return await FetchInternal(key).ConfigureAwait(false);
        }

        public async Task<string> AppendTo<TProperty>(RedisValue id, Expression<Func<T, TProperty>> expression, RedisValue appendValue)
        {
            var propertyName = _parser.GetPropertyName(expression);

            var key = _storageKeyGenerator.GetKey(_typeDefinition, id);

            var current = await _database.HashGetAsync(key, propertyName).ConfigureAwait(false);
            current = $"{current}{appendValue}";

            await _database.HashSetAsync(key, propertyName, current).ConfigureAwait(false);

            return current;
        }

        public async Task<long> Increment<TProperty>(RedisValue id, Expression<Func<T, TProperty>> expression, long incrementBy = 1)
        {
            var key = _storageKeyGenerator.GetKey(_typeDefinition, id);

            var propertyName = _parser.GetPropertyName(expression);

            return await _database.HashIncrementAsync(key, propertyName, incrementBy).ConfigureAwait(false);
        }

        public async Task<long> Decrement<TProperty>(RedisValue id, Expression<Func<T, TProperty>> expression, long decrementBy = 1)
        {
            var key = _storageKeyGenerator.GetKey(_typeDefinition, id);

            var propertyName = _parser.GetPropertyName(expression);

            return await _database.HashDecrementAsync(key, propertyName, decrementBy).ConfigureAwait(false);
        }

        public async Task<bool> Exists(RedisValue id)
        {
            var key = _storageKeyGenerator.GetKey(_typeDefinition, id);

            return await _database.KeyExistsAsync(key).ConfigureAwait(false);
        }

        public IObservable<T> FetchAll()
        {
            return Observable.Create<T>(
                async obs =>
                {
                    var clusterIndex = _storageKeyGenerator.GetClusteredIndexKey(_typeDefinition);
                    var keys = await _database.SortedSetRangeByRankAsync(clusterIndex).ConfigureAwait(false);

                    await FetchAllInternal(keys, obs);
                });
        }

        public IObservable<T> FetchAllWhere(Expression<Func<T, bool>> filter)
        {
            Tuple<SetOperation, RedisKey[]> result = ParseFilter(filter);

            var operation = result.Item1;
            var indexes = result.Item2;

            return Observable.Create<T>(
                async obs =>
                {
                    RedisValue[] keys;
                    if (indexes.Length == 1)
                        keys = await _database.SetMembersAsync(indexes[0]).ConfigureAwait(false);
                    else
                        keys = await _database.SetCombineAsync(operation, indexes[0], indexes[1]).ConfigureAwait(false);

                    await FetchAllInternal(keys, obs);
                });
        }

        public async Task<long> CountAll()
        {
            var clusterIndex = _storageKeyGenerator.GetClusteredIndexKey(_typeDefinition);
            return await _database.SortedSetLengthAsync(clusterIndex).ConfigureAwait(false);
        }

        public async Task<long> CountAllWhere(Expression<Func<T, bool>> filter)
        {
            Tuple<SetOperation, RedisKey[]> result = ParseFilter(filter);

            var operation = result.Item1;
            var indexes = result.Item2;

            if (indexes.Length == 1)
                return await _database.SetLengthAsync(indexes[0]).ConfigureAwait(false);
            else
                return (await _database.SetCombineAsync(operation, indexes[0], indexes[1]).ConfigureAwait(false)).LongCount();
        }

        public IDatabase GetDatabase() => _database;

        #region collapsed

        //private SetOperation ParseFilterExpression(Expression expression, List<RedisKey> indexes, int logicDepth, RedisValue propVal, Type propType, SetOperation operation)
        //{
        //    if(logicDepth > 2)
        //    {
        //        Console.WriteLine($"Current depth: {logicDepth}");
        //        throw new InvalidFilterExpression("You can only combine two expressions in a filter.");
        //    }

        //    if (expression is MemberExpression property)
        //    {
        //        if (propType == typeof(bool) && (property.Type != typeof(bool) && property.Type != typeof(bool?)))
        //            throw new InvalidFilterExpression($"Non boolean property expressions are not supported: '{expression}'.");

        //        var member = _typeDefinition.IndexedFields.Where(f => f.Name == property.Member.Name).FirstOrDefault();
        //        if (member == null) throw new InvalidFilterExpression($"{property.Member.Name} is not indexed");

        //        var indexKey = _storageKeyGenerator.GetFieldIndexKey(_typeDefinition, member, propVal);
        //        indexes.Add(indexKey);

        //        return operation;
        //    }
        //    else if (expression is UnaryExpression unary)
        //    {
        //        if (propType == typeof(int) && unary.NodeType == ExpressionType.Convert)
        //        {
        //            property = unary.Operand as MemberExpression;

        //            var member = _typeDefinition.IndexedFields.Where(f => f.Name == property.Member.Name).FirstOrDefault();
        //            if (member == null) throw new InvalidFilterExpression($"{property.Member.Name} is not indexed");

        //            propVal = _valueConverter.FromObject(Enum.ToObject(member.Type, (int) propVal), member.Type);
        //            var indexKey = _storageKeyGenerator.GetFieldIndexKey(_typeDefinition, member, propVal);
        //            indexes.Add(indexKey);

        //            return operation;
        //        }
        //        else if (unary.NodeType == ExpressionType.Not)
        //            return ParseFilterExpression(unary.Operand, indexes, logicDepth, false, typeof(bool), operation);

        //        throw new InvalidFilterExpression($"Only '!Boolean' and enum unary expressions are supported: '{expression}'.");
        //    }
        //    else if(expression is BinaryExpression comparison)
        //    {
        //        if (comparison.NodeType == ExpressionType.Equal)
        //        {
        //            propType = comparison.Right.Type;

        //            object value = Expression.Lambda(comparison.Right).Compile().DynamicInvoke();
        //            propVal = _valueConverter.FromObject(value, propType);

        //            return ParseFilterExpression(comparison.Left, indexes, logicDepth, propVal, propType, operation);
        //        }
        //        else if (comparison.NodeType == ExpressionType.NotEqual)
        //        {
        //            if (comparison.Left.Type != typeof(bool))
        //                throw new InvalidFilterExpression($"Filter expression operator '{comparison.NodeType}' is only supported on booleans.");

        //            if (comparison.Right is ConstantExpression constant)
        //            {
        //                return ParseFilterExpression(comparison.Left, indexes, logicDepth, !(bool)constant.Value, typeof(bool), operation);
        //            }

        //            throw new InvalidFilterExpression($"Unsupported expression: '{expression}'.");
        //        }
        //        else if (comparison.Right is MemberExpression || comparison.Right is UnaryExpression)
        //        {
        //            ParseFilterExpression(comparison.Right, indexes, logicDepth + 1, true, typeof(bool), operation);
        //        }
        //        else if (comparison.Right is BinaryExpression)
        //        {
        //            switch (comparison.NodeType)
        //            {
        //                case ExpressionType.AndAlso:
        //                    operation = SetOperation.Intersect;
        //                    break;
        //                case ExpressionType.OrElse:
        //                    operation = SetOperation.Union;
        //                    break;
        //                default:
        //                    throw new InvalidFilterExpression($"Filter expression operator '{comparison.NodeType}' is not supported.");
        //            }

        //            ParseFilterExpression(comparison.Right, indexes, logicDepth + 1, true, typeof(bool), operation);
        //        }
        //        else
        //        {
        //            throw new InvalidFilterExpression($"Unsupported operation: '{comparison.NodeType}'.");
        //        }

        //        switch (comparison.NodeType)
        //        {
        //            case ExpressionType.AndAlso:
        //                operation = SetOperation.Intersect;
        //                break;
        //            case ExpressionType.OrElse:
        //                operation = SetOperation.Union;
        //                break;
        //            default:
        //                throw new InvalidFilterExpression($"Filter expression operator '{comparison.NodeType}' is not supported.");
        //        }

        //        if (comparison.Left is MemberExpression || comparison.Left is UnaryExpression)
        //        {
        //            return ParseFilterExpression(comparison.Left, indexes, logicDepth + 1, true, typeof(bool), operation);
        //        }
        //        else if (comparison.Left is BinaryExpression)
        //        {
        //            return ParseFilterExpression(comparison.Left, indexes, logicDepth + 1, true, typeof(bool), operation);
        //        }

        //        throw new InvalidFilterExpression($"Unsupported expression: '{expression}'.");
        //    }
        //    else
        //    {
        //        throw new InvalidFilterExpression($"Unsupported expression: '{expression}'.");
        //    }
        //}

        #endregion collapsed

        #region fetch variants

        public async Task<T> Fetch(string id)
        {
            return await Fetch((RedisValue)id).ConfigureAwait(false);
        }

        public async Task<T> Fetch(byte[] id)
        {
            return await Fetch((RedisValue)id).ConfigureAwait(false);
        }

        public async Task<T> Fetch(int id)
        {
            return await Fetch((RedisValue)id.ToString()).ConfigureAwait(false);
        }

        public async Task<T> Fetch(long id)
        {
            return await Fetch((RedisValue)id.ToString()).ConfigureAwait(false);
        }

        public async Task<T> Fetch(Guid id)
        {
            return await Fetch((RedisValue)id.ToString()).ConfigureAwait(false);
        }

        #endregion fetch variants

        #region internals

        private Tuple<SetOperation, RedisKey[]> ParseFilter(Expression<Func<T, bool>> filter)
        {
            var tree = _parser.ParseFilter(filter);

            RedisKey[] indexes;
            SetOperation operation;
            if (tree is UnaryFilterExpression unary)
            {
                indexes = new RedisKey[] { FilterToKey(unary.Filter) };
                operation = SetOperation.Difference;//difference isn't used
            }
            else if (tree is BinaryFilterExpression binary)
            {
                indexes = new RedisKey[] { FilterToKey(binary.Left), FilterToKey(binary.Right) };
                operation = binary.Comparison == ComparisonOperator.And ? SetOperation.Intersect : SetOperation.Union;
            }
            else throw new InvalidFilterExpression($"Filter is invalid '{filter}'.");

            return Tuple.Create(operation, indexes);
        }

        private RedisKey FilterToKey(Filter filter)
        {
            var member = _typeDefinition.IndexedFields.Where(f => f.Name == filter.OperandName).FirstOrDefault();
            if (member == null) throw new InvalidFilterExpression($"{filter.OperandName} is not indexed");

            RedisValue value;

            if (member.Type.GetTypeInfo().IsEnum)
            {
                value = _valueConverter.FromObject(Enum.ToObject(member.Type, (int)filter.RightValue), member.Type);
            }
            else
            {
                value = _valueConverter.FromObject(filter.RightValue, member.Type);
            }

            if (filter.Operator == FilterOperator.NotEquals)
            {
                if (member.Type != typeof(bool))
                    throw new InvalidFilterExpression($"The 'Not Equals' operation is only supported on booleans.");

                value = !(bool)value;
            }

            return _storageKeyGenerator.GetFieldIndexKey(_typeDefinition, member, value);
        }

        private async Task FetchAllInternal(RedisValue[] keys, IObserver<T> obs)
        {
            foreach (var key in keys)
            {
                var item = await FetchInternal((string)key);
                obs.OnNext(item);
            }

            obs.OnCompleted();
        }

        private async Task<T> FetchInternal(RedisKey key)
        {
            var hash = await _database.HashGetAllAsync(key).ConfigureAwait(false);

            if (hash.Length == 0) return default(T);

            return _mapper.InstanceFor<T>(_typeDefinition, hash);
        }

        #endregion internals
    }
}