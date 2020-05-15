using Me.Bartecki.RepoCounter.Domain.Model;
using Me.Bartecki.RepoCounter.Domain.Services.Interfaces;
using Me.Bartecki.RepoCounter.Infrastructure.Model;
using Optional;
using System.Linq;
using System.Threading.Tasks;

namespace Me.Bartecki.RepoCounter.Domain.Services.Decorators
{
    public class OrderedLettersStatisticsDecorator : IRepoStatisticsService
    {
        private readonly IRepoStatisticsService _innerService;

        public OrderedLettersStatisticsDecorator(IRepoStatisticsService innerService)
        {
            _innerService = innerService;
        }

        public async Task<Option<UserStatistics, RepoCounterApiException>> GetRepositoryStatisticsAsync(string username)
        {
            var input = await _innerService.GetRepositoryStatisticsAsync(username);
            input.Map(stats => stats.Letters = stats.Letters
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value));
            return input;
        }
    }
}
