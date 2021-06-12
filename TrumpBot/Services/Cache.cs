using System;
using NLog;

namespace TrumpBot.Services
{
    public static class Cache
    {
        public static void Set(string key, object cacheObject, DateTimeOffset expiration)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug($"Storing {key} until {expiration}");
            AppDomain.CurrentDomain.SetData(key, new CacheModel {Data = cacheObject, Expiration = expiration});
        }

        public static T Get<T>(string key)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug($"Retriving {key} from cache and casting to {typeof(T)}");
            if (AppDomain.CurrentDomain.GetData(key) is not CacheModel cacheItem)
            {
                logger.Debug($"{key} returned something that doesn't look like CacheModel, returning default");
                return default;
            }

            if (cacheItem.Expiration > DateTimeOffset.Now)
            {
                logger.Debug($"{key} hasn't expired yet, casting to {typeof(T)} and returning");
                return (T) cacheItem.Data;
            }

            // GC the item so it doesn't sit until cache is renewed
            cacheItem.Data = null;
            AppDomain.CurrentDomain.SetData(key, cacheItem);
            logger.Debug($"{key} has expired, set data to null to free memory and returning default to caller");

            return default;
        }

        private class CacheModel
        {
            public object Data { get; set; }
            public DateTimeOffset Expiration { get; set; }
        }
    }
}