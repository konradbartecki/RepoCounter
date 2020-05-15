namespace Me.Bartecki.RepoCounter.Infrastructure.Model
{
    public class Repository
    {
        public string Name { get; set; }
        public int Stargazers { get; set; }
        public int Watchers { get; set; }
        public int Forks { get; set; }
        public int Size { get; set; }
    }
}
