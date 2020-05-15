# RepoCounter task

Example backend ASP.NET Web API that returns statistics for a single user for (theoretically) any source control hosting website, but currently only GitHub is supported.

#### Deployment: 

1. Open https://github.com/settings/tokens
2. Generate new personal access tokens
3. Update GITHUB_TOKEN in with your new personal access token
    -   Me.Bartecki.RepoCounter.Api/appsettings.json 
    -   Me.Bartecki.RepoCounter.Api.IntegrationTests/appsettings.json