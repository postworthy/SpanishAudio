using SpanishAudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpanishAudio.Services
{
    public static class DynamicSpanishAudio
    {
        private const string PHRASE_DIR = "phrases/";
        private const string KNOWN_WORDS = "Resources/KnownWords.csv";
        private static Regex PunctuationRegex = new Regex(@"(\p{P})|\t|\n|\r", RegexOptions.Compiled);
        private static Regex WhiteSpaceRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);
        public static void CreateKnownContent()
        {
            var knownWordsWithStrengths = System.IO.File.ReadAllText(KNOWN_WORDS).Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new
                {
                    word = x.Split(',')[0],
                    strength = int.Parse(x.Split(',')[1])
                }).OrderByDescending(x => x.strength).ThenBy(x => x.word);

            var knownWords = new HashSet<string>(knownWordsWithStrengths.Select(x => x.word));
            var matches = new List<Tuple<Phrase, IEnumerable<string>, double>>(2000);
            var files = System.IO.Directory.GetFiles(PHRASE_DIR);
            foreach (var file in files)
            {
                var verb = file.Split('/')[1].Split('.')[0];
                var phrases = PhraseFile.GetPhrases(verb);

                foreach (var phrase in phrases)
                {
                    var phraseWords = WhiteSpaceRegex.Replace(PunctuationRegex.Replace(phrase.Spanish, " "), " ").ToLower().Split(' ').Select(x => x.Trim());
                    var matchPercent = phraseWords.Where(x => knownWords.Contains(x)).Count() / (1.0 * phraseWords.Count());
                    if (matchPercent >= 0.70)
                        matches.Add(new Tuple<Phrase, IEnumerable<string>, double>(phrase, phraseWords, matchPercent));
                }
            }

            matches = matches.OrderByDescending(x => {
                var sum = 0.0;
                var count = 0.0;
                foreach(var val in x.Item2)
                {
                    count++;
                    sum += knownWordsWithStrengths.Where(w => w.word == val).Select(w=>w.strength).FirstOrDefault();
                }
                return (sum / count);// *x.Item3; 
            }).ThenBy(x => x.Item1.Spanish.Length).ToList();
            int levels = (int)Math.Ceiling(matches.Count() / 100.0);
            for (int i = 0; i < levels; i++)
            {
                var level = matches.Skip(i * 100).Take(100).Select(x => x.Item1).ToList();
                PhraseFile.SaveDynamicAudio(i + 1, level);
            }
        }
    }
}
