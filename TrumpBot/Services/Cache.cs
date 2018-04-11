using System;
using System.Runtime.Caching;

namespace TrumpBot.Services
{
    public static class Cache
    {
        public static void Set(string key, object cacheObject, DateTimeOffset expiration)
        {
            ObjectCache cache = MemoryCache.Default;

            cache.Set(key, cacheObject, expiration);
        }

        public static object Get(string key)
        {
            ObjectCache cache = MemoryCache.Default;

            return cache[key];
        }

        public static T Get<T>(string key)
        {
            ObjectCache cache = MemoryCache.Default;

            return (T) cache[key];
        }
    }
}