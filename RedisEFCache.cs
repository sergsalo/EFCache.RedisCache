using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace EFCache.RedisCache
{
    public class RedisEFCache : ICache
    {
        private readonly ConnectionMultiplexer mRedis;

        public RedisEFCache(ConnectionMultiplexer multiplexer)
        {
            mRedis = multiplexer;
        }

        public bool GetItem(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            var db = mRedis.GetDatabase();
            value = db.Get<object>(key);
            return value != null;
        }

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration,
            DateTimeOffset absoluteExpiration)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (dependentEntitySets == null)
            {
                throw new ArgumentNullException("dependentEntitySets");
            }

            var db = mRedis.GetDatabase();
            //TODO: set expiration time

            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(key));
            foreach (var entitySet in dependentEntitySets)
            {
                var entitySetKey = GetEntitySetKey(entitySet);
                tran.SetAddAsync(entitySetKey, key);
            }
            tran.SetAsync(key, value);
            tran.Execute(); //transaction will be rolled back 
        }

        public void InvalidateSets(IEnumerable<string> entitySets)
        {
            if (entitySets == null)
            {
                throw new ArgumentNullException("entitySets");
            }

            var db = mRedis.GetDatabase();
            foreach (var entitySet in entitySets)
            {
                var entitySetKey = GetEntitySetKey(entitySet);
                var keys = db.SetMembers(entitySetKey);
                if (keys.Length > 0)
                {
                    foreach (var key in keys)
                    {
                        db.KeyDelete((string) key);
                    }
                    db.SetRemove(entitySetKey, keys);
                }
            }
        }

        public void InvalidateItem(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            var db = mRedis.GetDatabase();
            db.KeyDelete(key);
        }

        private static string GetEntitySetKey(string entitySet)
        {
            return "ef_" + entitySet;
        }
    }
}