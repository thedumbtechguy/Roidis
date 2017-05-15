using Roidis.Proxy.Object;

namespace Roidis
{
    public interface IRoid
    {
        IObjectProxy<T> From<T>(int database = 0) where T : new();
    }
}