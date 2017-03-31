using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Roidis.Proxy.Object
{
    public interface IObjectProxy<T> where T : new()
    {
        IObservable<T> FetchAll();
        IObservable<T> FetchAllWhere<TProperty>(Expression<Func<T, TProperty>> expression);
        Task<T> Fetch(RedisValue id);

        Task<T> Fetch(string id);

        Task<T> Fetch(byte[] id);

        Task<T> Fetch(int id);

        Task<T> Fetch(long id);

        Task<T> Fetch(Guid id);

        Task<T> Save(T obj);
    }
}
