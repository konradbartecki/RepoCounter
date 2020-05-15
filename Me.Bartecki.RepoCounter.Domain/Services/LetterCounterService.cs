using Me.Bartecki.RepoCounter.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Me.Bartecki.RepoCounter.Domain.Services
{
    public class LetterCounterService : ILetterCounterService
    {
        public Dictionary<char, int> CountLetters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input string cannot be null or whitespace", nameof(input));

            var charArray = input
                .ToLower()
                .ToCharArray();
            var letters = charArray
                .Where(c => char.IsLetter(c))
                .GroupBy(c => c)
                .ToDictionary(x => x.Key, x => x.Count());
            return letters;
        }
    }
}
