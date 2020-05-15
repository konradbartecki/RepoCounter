using Me.Bartecki.RepoCounter.Domain.Model;
using Me.Bartecki.RepoCounter.Domain.Services.Interfaces;
using Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores;
using Me.Bartecki.RepoCounter.Infrastructure.Model;
using Optional;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Me.Bartecki.RepoCounter.Domain.Services
{
    /// <summary>
    /// Note: There are some discussable degrees of how SOLIDified code should be, the same goes for unit test coverage.
    /// Here in this situation I decided to not split that code into another class,
    /// thus I am breaking the "Single responsibility" rule.
    /// If the complexity of statistic calculation would be higher,
    /// I would split this logic into another class like RepoStatisticCalculationService
    /// and maybe RepoStatistic service that depends on both
    /// RepoStatisticCalculationService and ILetterCounterService.
    /// </summary>
    public class RepoStatisticsService : IRepoStatisticsService
    {
        private readonly IRepositoryStoreService _repositoryStore;
        private readonly ILetterCounterService _letterCounterService;

        public RepoStatisticsService(IRepositoryStoreService repositoryStore, ILetterCounterService letterCounterService)
        {
            _repositoryStore = repositoryStore;
            _letterCounterService = letterCounterService;
        }

        public async Task<Option<UserStatistics, RepoCounterApiException>> GetRepositoryStatisticsAsync(string username)
        {
            var optionTaskResult = await _repositoryStore.GetUserRepositoriesAsync(username);
            return optionTaskResult.Match(
                some: repos =>
                {
                    var repositories = repos.ToList();
                    if (repositories.Any())
                    {
                        return Option.Some<UserStatistics, RepoCounterApiException>(
                            TransformStatistics(username, repositories));
                    }
                    else
                    {
                        return Option.None<UserStatistics, RepoCounterApiException>(
                            new RepoCounterApiException(ErrorCodes.UserHasNoRepositories));
                    }
                },
                none: exception => Option.None<UserStatistics, RepoCounterApiException>(exception)
            );
        }

        private UserStatistics TransformStatistics(string username, List<Repository> repos)
        {
            var ret = new UserStatistics { Owner = username };
            foreach (var repo in repos)
            {
                //Temporarily sum everything on a returned object,
                //we will divide it later to get an average.
                ret.AverageForks += repo.Forks;
                ret.AverageStargazers += repo.Stargazers;
                ret.AverageWatchers += repo.Watchers;
                ret.AverageSize += repo.Size;
                var thisRepoLetterCount = _letterCounterService.CountLetters(repo.Name);

                //Merge dictionaries
                foreach (var pair in thisRepoLetterCount)
                {
                    if (ret.Letters.ContainsKey(pair.Key))
                        ret.Letters[pair.Key] += pair.Value;
                    else
                        ret.Letters.Add(pair.Key, pair.Value);
                }
            }

            var repoCount = (double)repos.Count;
            ret.AverageForks = ret.AverageForks / repoCount;
            ret.AverageStargazers = ret.AverageStargazers / repoCount;
            ret.AverageWatchers = ret.AverageWatchers / repoCount;
            ret.AverageSize = ret.AverageSize / repoCount;
            return ret;
        }
    }
}
