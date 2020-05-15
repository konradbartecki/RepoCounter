using System.IO;
using System.Reflection;

namespace Me.Bartecki.RepoCounter.Infrastructure.Services
{
    public class EmbeddedResourceService : IEmbeddedResourceService
    {
        public string GetResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
