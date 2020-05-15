using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using System.Collections.Generic;

namespace Me.Bartecki.RepoCounter.Infrastructure.Integrations.RepositoryStores.GitHub
{
    /// <summary>
    /// This class is partial, so I can encapsulate model and set it as private.
    /// It does not need to be seen outside of GitHubIntegrationService
    /// </summary>
    public partial class GitHubIntegrationService
    {
        private class RootResponse : ObjectGraphType
        {
            public User User { get; set; }
        }

        private class User
        {
            public Repositories Repositories { get; set; }
        }

        private class GitHubRepository
        {
            public string Name { get; set; }
            public int DiskUsage { get; set; }
            public int ForkCount { get; set; }
            public TotalCountHolder Stargazers { get; set; }
            public TotalCountHolder Watchers { get; set; }
        }

        private class TotalCountHolder
        {
            public int TotalCount { get; set; }
        }

        private class Repositories
        {
            public PageInfo PageInfo { get; set; }
            public IEnumerable<GitHubRepository> Nodes { get; set; }
        }
    }
}
