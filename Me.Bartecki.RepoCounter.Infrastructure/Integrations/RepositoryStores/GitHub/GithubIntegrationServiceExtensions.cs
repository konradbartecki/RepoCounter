using Microsoft.Extensions.DependencyInjection;
using System;

namespace Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores.GitHub
{
    public static class GithubIntegrationServiceExtensions
    {
        public static IServiceCollection AddGithubIntegration(this IServiceCollection collection, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Github token cannot be null or whitespace");
            collection.AddHttpClient<IRepositoryStoreService, GitHubIntegrationService>(client =>
            {
                client.DefaultRequestHeaders
                    .Add("Authorization", $"token {token}");
                //User-agent is mandatory for GitHub API calls
                client.DefaultRequestHeaders.Add("User-Agent", "konradbartecki");
            });
            return collection;
        }
    }
}
