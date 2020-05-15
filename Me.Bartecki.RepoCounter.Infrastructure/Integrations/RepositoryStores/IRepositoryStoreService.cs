using Optional;
using System.Collections.Generic;
using System.Threading.Tasks;
using Me.Bartecki.RepoCounter.Infrastructure.Model;

namespace Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores
{
    public interface IRepositoryStoreService
    {
        Task<Option<IEnumerable<Repository>, RepoCounterApiException>> GetUserRepositoriesAsync(string username);
    }
}