# EFCache.RedisCache

RedisCache implementation for Second Level Cache for Entity Framework 6.1 (https://efcache.codeplex.com/) ICache interface.

Nuget package will be created soon.

Does not support cache expiration just yet. 

How to use?

```csharp
public class Configuration : DbConfiguration
{
  public Configuration()
  {
    var redisCache = new RedisEFCache(ConnectionMultiplexer.Connect(RedisCacheConnectionString));
    
    var transactionHandler = new CacheTransactionHandler(redisCache);

    AddInterceptor(transactionHandler);

    var cachingPolicy = new CachingPolicy();

    Loaded +=
      (sender, args) => args.ReplaceService<DbProviderServices>(
        (s, _) => new CachingProviderServices(s, transactionHandler, cachingPolicy));
  }
}
```
I found this package "silentbobbert/EFCache.Redis" but it seems not to support distributed nature of Redis, as internally caches dependent entity set names in a dictionary instead of in Redis itself.
As the result state is not persistent with application restarts or when new instances of application are brought into existence. But it gave me a start.

