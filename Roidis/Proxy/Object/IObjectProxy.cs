﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Proxy.Object
{
    public interface IObjectProxy<T> where T : new()
    {
        Task<long> Increment<TProperty>(RedisValue id, Expression<Func<T, TProperty>> expression, long incrementBy = 1);
        Task<long> Decrement<TProperty>(RedisValue id, Expression<Func<T, TProperty>> expression, long decrementBy = 1);

        IObservable<T> FetchAllWhere(Expression<Func<T, bool>> expression);
        Task<long> CountAllWhere(Expression<Func<T, bool>> expression);

        IObservable<T> FetchAll();
        Task<long> CountAll();


        Task<T> Fetch(RedisValue id);

        Task<T> Fetch(string id);

        Task<T> Fetch(byte[] id);

        Task<T> Fetch(int id);

        Task<T> Fetch(long id);

        Task<T> Fetch(Guid id);

        Task<T> Save(T obj, bool saveChangedOnly = true);
    }
}
