using SpanishAudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SpanishAudio.Services
{
    public static class SpanishPhraseScraper
    {
        private const string VERB_LIST_URL = "http://users.ipfw.edu/jehle/VERBLIST.HTM";
        private const string PHRASE_BASE_URL = "http://www.123teachme.com/translated_sentences/sp/";
        public static IEnumerable<string> GetVerbs()
        {
            var verbDoc = new HtmlAgilityPack.HtmlDocument();
            using (var verbClient = new WebClient())
            {
                verbClient.Encoding = Encoding.UTF8;
                verbDoc.LoadHtml(verbClient.DownloadString(VERB_LIST_URL));
                return verbDoc.DocumentNode.SelectNodes("//a")
                    .Where(x => x.Attributes.Where(y => y.Name == "href" && y.Value.ToLower().StartsWith("courses/verbs") && y.Value.ToLower().EndsWith(".htm")).Count() > 0)
                    .Select(x => HttpUtility.HtmlDecode(x.InnerText)).Distinct();
            }
        }
        public static IEnumerable<Phrase> GetPhrases(string verb)
        {
            var phraseDoc = new HtmlAgilityPack.HtmlDocument();
            using (var phraseClient = new WebClient())
            {
                //try
                //{
                    phraseClient.Encoding = Encoding.UTF8;
                    phraseDoc.LoadHtml(phraseClient.DownloadString(PHRASE_BASE_URL + verb));
                    var pairs = phraseDoc.DocumentNode.SelectNodes("//div[@class=\"translated-sentence-pair\"]");
                    if (pairs != null)
                    {
                        var phrases = pairs
                            .Select(x => new Phrase()
                            {
                                SpanishKeyword = verb,
                                Spanish = x.SelectSingleNode("div[@class=\"spanish-sentence\"]").InnerText,
                                English = x.SelectSingleNode("div[@class=\"english-sentence\"]").InnerText.Replace("’", "'").Replace("&#x27;", "'"),
                                SpanishAudio = GetMp3Data(x.SelectSingleNode("div[@class=\"spanish-sentence\"]")),
                                EnglishAudio = null
                            }).ToList();

                        foreach (var phrase in phrases)
                        {
                            if (phrase.SpanishAudio == null)
                                phrase.AllowModifySpanish = true;

                            phrase.AllowModifyEnglish = true;
                        }

                        GetTTS(phrases);


                        return phrases;
                    }
                //}
                //catch { }
            }
            return new List<Phrase>();
        }

        public static void GetTTS(List<Phrase> phrases)
        {
            foreach (var phrase in phrases)
            {
                if (phrase.AllowModifySpanish)
                {
                    var es = TextToSpeech.GetSpanishAudio(phrase.Spanish);
                    if (es != null)
                    {
                        phrase.SpanishAudio = es;
                        phrase.AllowModifySpanish = false;
                    }
                }
                if (phrase.AllowModifyEnglish)
                {
                    var en = TextToSpeech.GetEnglishAudio(phrase.English);
                    if (en != null)
                    {
                        phrase.EnglishAudio = en;
                        phrase.AllowModifyEnglish = false;
                    }
                }
            }
        }

        private static byte[] GetMp3Data(HtmlAgilityPack.HtmlNode htmlNode)
        {
            var anchorNode = htmlNode.SelectSingleNode("a");

            if (anchorNode != null)
            {
                var attr = anchorNode.Attributes.FirstOrDefault(x => x.Name == "href");
                if (attr != null && attr.Value.ToLower().EndsWith("mp3"))
                {
                    using (var mp3Client = new WebClient())
                    {
                        return mp3Client.DownloadData(attr.Value);
                    }
                }
            }

            return null;
        }
    }
}
