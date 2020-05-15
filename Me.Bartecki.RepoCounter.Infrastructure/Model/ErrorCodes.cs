namespace Me.Bartecki.RepoCounter.Infrastructure.Model
{
    public enum ErrorCodes
    {
        UnhandledException = 0,
        UserNotFound,
        UserHasNoRepositories,
        RepositorySource_UnableToReach,
    }
}
