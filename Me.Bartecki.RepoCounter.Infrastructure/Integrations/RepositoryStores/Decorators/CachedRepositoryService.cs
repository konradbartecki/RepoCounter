using Microsoft.Extensions.Caching.Memory;
using Optional;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Me.Bartecki.RepoCounter.Infrastructure.Model;

namespace Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores.Decorators
{
    public class CachedRepositoryDecorator : IRepositoryStoreService
    {
        private readonly IRepositoryStoreService _innerRepositoryService;
        private readonly IMemoryCache _memoryCache;

        public CachedRepositoryDecorator(IRepositoryStoreService innerRepositoryService, IMemoryCache memoryCache)
        {
            _innerRepositoryService = innerRepositoryService;
            _memoryCache = memoryCache;
        }
        public async Task<Option<IEnumerable<Repository>, RepoCounterApiException>> GetUserRepositoriesAsync(string username)
        {
            return await _memoryCache.GetOrCreateAsync(username, async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return await _innerRepositoryService.GetUserRepositoriesAsync(username);
            });
        }
    }
}
