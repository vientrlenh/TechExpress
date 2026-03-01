using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using TechExpress.Repository.CustomExceptions;

namespace TechExpress.Service.Utils
{
    public class RedisUtils
    {
        private readonly IDistributedCache _redisCache;
        private readonly IConnectionMultiplexer _redisConnection;

        public RedisUtils(IDistributedCache redisCache, IConnectionMultiplexer redisConnection)
        {
            _redisCache = redisCache;
            _redisConnection = redisConnection;
        }

        public async Task StoreStringData(string key, string data, TimeSpan expiration)
        {
            await CheckRedisAvailable();
            await _redisCache.SetStringAsync(key, data, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
        }

        public async Task StoreLongData(string key, long data, TimeSpan expiration)
        {
            string dataStr = data.ToString();
            await StoreStringData(key, dataStr, expiration);
        }


        public async Task StoreGuidData(string key, Guid data, TimeSpan expiration)
        {
            string dataStr = data.ToString();
            await StoreStringData(key, dataStr, expiration);
        }


        public async Task<string?> GetStringDataFromKey(string key)
        {
            await CheckRedisAvailable();
            return await _redisCache.GetStringAsync(key);
        }

        public async Task RemoveAsync(string key)
        {
            await CheckRedisAvailable();
            await _redisCache.RemoveAsync(key);
        }

        public async Task CheckRedisAvailable()
        {
            try
            {
                var db = _redisConnection.GetDatabase();
                var pong = await db.PingAsync();
            }
            catch (RedisConnectionException)
            {
                throw new ServiceUnavailableException("Redis server hiện tại không khả dụng.");
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi không xác định.", ex);
            }
        }

        public async Task<bool> TrySetStringIfNotExists(string key, string data, TimeSpan expiration)
        {
            await CheckRedisAvailable();
            var db = _redisConnection.GetDatabase();
            return await db.StringSetAsync(key, data, expiration, When.NotExists);
        }

    }
}
