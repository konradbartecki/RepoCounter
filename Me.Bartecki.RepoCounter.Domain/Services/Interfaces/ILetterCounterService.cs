using System.Collections.Generic;

namespace Me.Bartecki.RepoCounter.Domain.Services.Interfaces
{
    public interface ILetterCounterService
    {
        Dictionary<char, int> CountLetters(string input);
    }
}