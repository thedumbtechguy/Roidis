using FastMember;
using Roidis.Exception;
using Roidis.Proxy;
using Roidis.Proxy.Object;
using Roidis.Service.Converter;
using Roidis.Service.KeyGenerator;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Roidis
{
    public interface IRoid
    {
        IObjectProxy<T> From<T>(int database = 0) where T : new();  
    }
}
