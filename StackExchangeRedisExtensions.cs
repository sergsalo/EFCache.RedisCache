﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using StackExchange.Redis;

namespace EFCache.RedisCache
{
    public static class StackExchangeRedisExtensions
    {
        public static T Get<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        public static object Get(this IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        public static void Set(this IDatabase cache, string key, object value, TimeSpan? expiry = null)
        {
            cache.StringSet(key, Serialize(value), expiry);
        }

        public static void SetAsync(this ITransaction transaction, string key, object value, TimeSpan? expiry = null)
        {
            transaction.StringSetAsync(key, Serialize(value), expiry);
        }

        private static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                var objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        private static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(stream))
            {
                var result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
    }

}