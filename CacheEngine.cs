using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;


namespace Assignment1
{
    class CacheEngine
    {

        static readonly ObjectCache cache =  MemoryCache.Default;


        public static  T Get<T>(string key) where T : class
            {
            try
            {
                return (T)cache[key];
            }
            catch
            {
                return null;
            }

        }

        public static void Add<T>(string key,T item)
        {
            cache.Add(key, item, DateTimeOffset.Now.AddHours(1));
        }

    }
}
