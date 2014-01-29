using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpanishAudio
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                ShowHelp();
            else
            {
                if (args[0].StartsWith("-f") || args[0].StartsWith("-s"))
                {
                    var verbs = SpanishAudio.Services.SpanishPhraseScraper.GetVerbs();
                    foreach (var verb in verbs)
                    {
                        IEnumerable<SpanishAudio.Models.Phrase> phrases;
                        if (!SpanishAudio.Services.PhraseFile.Exists(verb) || args[0].EndsWith("o"))
                        {
                            phrases = SpanishAudio.Services.SpanishPhraseScraper.GetPhrases(verb);

                            SpanishAudio.Services.PhraseFile.SavePhrases(verb, phrases);
                            if (args[0].StartsWith("-f"))
                                SpanishAudio.Services.PhraseFile.SaveAudio(verb, phrases.ToList(), args[0].EndsWith("o"));
                            Console.WriteLine(verb + " saved!");
                        }
                    }
                }
                else if (args[0].StartsWith("-d"))
                {
                    SpanishAudio.Services.DynamicSpanishAudio.CreateKnownContent();
                }
                else if (args[0].StartsWith("-m"))
                {
                    var verbs = SpanishAudio.Services.SpanishPhraseScraper.GetVerbs();
                    foreach (var verb in verbs)
                    {
                        IEnumerable<SpanishAudio.Models.Phrase> phrases;
                        phrases = SpanishAudio.Services.PhraseFile.GetPhrases(verb);
                        SpanishAudio.Services.SpanishPhraseScraper.GetTTS(phrases.ToList());
                        SpanishAudio.Services.PhraseFile.SavePhrases(verb, phrases);
                        SpanishAudio.Services.PhraseFile.SaveAudio(verb, phrases.ToList());
                        Console.WriteLine(verb + " updated missing!");
                    }
                }
            }


        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: spanishaudio <command>");
            Console.WriteLine("Commands:");
            Console.WriteLine("-f\t\tScrape phrases generate phrase files and generate audio files");
            Console.WriteLine("-s\t\tScrape phrases and generate phrase files");
            Console.WriteLine("-m\t\tUpdate missing audio phrases and generate audio files");
            Console.WriteLine("-d [known word file]\t\tCreate audio files based on known words");
            Console.WriteLine("-o\t\tOveride existing, can be used with -f and -s and -d (-fo or -so or -do)");
        }
    }
}
