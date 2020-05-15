using GraphQL;
using GraphQL.Client.Http;
using Me.Bartecki.RepoCounter.Infrastructure.Model;
using Me.Bartecki.RepoCounter.Infrastructure.Services;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores.GitHub
{

    public partial class GitHubIntegrationService : IRepositoryStoreService
    {
        private const string GITHUB_URL = "https://api.github.com/graphql";
        private readonly GraphQLHttpClient _client;
        private readonly IEmbeddedResourceService _embeddedResourceService;

        public GitHubIntegrationService(HttpClient client, IEmbeddedResourceService embeddedResourceService)
        {
            _client = client.AsGraphQLClient(GITHUB_URL);
            _embeddedResourceService = embeddedResourceService;
        }

        private Repository Convert(GitHubRepository source)
        {
            var dest = new Repository();
            dest.Name = source.Name;
            dest.Size = source.DiskUsage;
            dest.Stargazers = source.Stargazers.TotalCount;
            dest.Watchers = source.Watchers.TotalCount;
            dest.Forks = source.ForkCount;
            return dest;
        }

        public async Task<Option<IEnumerable<Repository>, RepoCounterApiException>> GetUserRepositoriesAsync(string username)
        {
            string query = _embeddedResourceService.GetResource("Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores.GitHub.GitHubQuery.graphql");
            try
            {
                bool isNextPageAvailable = false;
                string nextPageId = null;

                List<GitHubRepository> repositories = new List<GitHubRepository>();
                do
                {
                    var request = new GraphQLHttpRequest(query,
                        new
                        {
                            username = username,
                            nextCursorId = nextPageId
                        });
                    var response = await _client.SendQueryAsync<RootResponse>(request);
                    if (response.Errors?.Any() == true)
                    {
                        return GetException(response);
                    }

                    repositories.AddRange(response.Data.User.Repositories.Nodes);

                    //Handle pagination
                    var pageInfo = response.Data.User.Repositories.PageInfo;
                    isNextPageAvailable = pageInfo.HasNextPage;
                    if (isNextPageAvailable)
                    {
                        nextPageId = pageInfo.EndCursor;
                    }
                } while (isNextPageAvailable);

                return Option.Some<IEnumerable<Repository>, RepoCounterApiException>(repositories.Select(Convert));
            }
            catch (GraphQLHttpException httpRequestException) when (
                httpRequestException.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                //Rethrow this exception, critical misconfiguration.
                throw;
            }
            catch (GraphQLHttpException httpRequestException)
            {
                var errorCode = ErrorCodes.RepositorySource_UnableToReach;
                var apiException = new RepoCounterApiException(
                    errorCode,
                    httpRequestException.Message,
                    httpRequestException);

                return Option.None<IEnumerable<Repository>, RepoCounterApiException>(apiException);
            }
        }

        private Option<IEnumerable<Repository>, RepoCounterApiException> GetException(
            GraphQLResponse<RootResponse> response)
        {
            bool userNotFound = response.Errors.First().Message.Contains("Could not resolve to a User");
            if (userNotFound)
            {
                return Option.None<IEnumerable<Repository>, RepoCounterApiException>(
                    new RepoCounterApiException(ErrorCodes.UserNotFound, response.Errors.First().Message));
            }
            else
            {
                var e = new Exception("Unhandled exception while requesting a GitHub repository");
                e.Data.Add("response", response);
                throw e;
            }
        }
    }
}
