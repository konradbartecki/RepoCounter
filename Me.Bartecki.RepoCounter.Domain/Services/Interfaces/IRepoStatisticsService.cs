using Me.Bartecki.RepoCounter.Domain.Model;
using Me.Bartecki.RepoCounter.Infrastructure.Model;
using Optional;
using System.Threading.Tasks;

namespace Me.Bartecki.RepoCounter.Domain.Services.Interfaces
{
    public interface IRepoStatisticsService
    {
        Task<Option<UserStatistics, RepoCounterApiException>> GetRepositoryStatisticsAsync(string username);
    }
}