using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SpanishAudio.Services
{
    public static class TextToSpeech
    {
        //private const string ONLINE_SPANISH_TTS_URL = "http://api.naturalreaders.com/v2/tts/?t={0}&r=19&s=1&requesttoken=3cf1dfe079c77dbb619914014c0d80f1&recognkey=72c62af9e82009ebac869afe3d508dff";
        //private const string ONLINE_SPANISH_TTS_URL = "http://204.178.9.51/tts/cgi-bin/nph-nvttsdemo";
        private const string ONLINE_SPANISH_TTS_URL = "http://translate.google.com/translate_tts?tl=es&q={0}";
        private const string ONLINE_ENGLISH_TTS_URL = "http://translate.google.com/translate_tts?tl=en&q={0}";
        public static byte[] GetSpanishAudio(string phrase)
        {
            /*
            var speaker = new SpeechSynthesizer();
            var voice = speaker.GetInstalledVoices(CultureInfo.GetCultureInfoByIetfLanguageTag("es-MX")).FirstOrDefault();
            if (voice != null)
            {
                speaker.Rate = 1;
                speaker.Volume = 100;
                speaker.SelectVoice(voice.VoiceInfo.Name);

                using (var output = new MemoryStream())
                {
                    speaker.SetOutputToWaveStream(output);
                    speaker.Speak(phrase);
                    return WavToMP3(output.ToArray());
                }
            }
            else
            {
                return GetSpanishAudioOnline(phrase);
            }

            return null;
             */

            return GetSpanishAudioOnline(phrase);
        }

        public static byte[] GetEnglishAudio(string phrase)
        {
            /*
            var speaker = new SpeechSynthesizer();
            var voice = speaker.GetInstalledVoices(CultureInfo.GetCultureInfoByIetfLanguageTag("en-US")).FirstOrDefault();
            if (voice != null)
            {
                speaker.Rate = 1;
                speaker.Volume = 100;
                speaker.SelectVoice(voice.VoiceInfo.Name);

                using (var output = new MemoryStream())
                {
                    speaker.SetOutputToWaveStream(output);
                    speaker.Speak(phrase);
                    return WavToMP3(output.ToArray());
                }
            }
            else
            {
                return GetEnglishAudioOnline(phrase);
            }
             * 
            return null;
             */

            return GetEnglishAudioOnline(phrase);
        }

        private static byte[] GetSpanishAudioOnline(string phrase)
        {
            try
            {
                var proxy = new SocksWebProxy(new ProxyConfig(
                    IPAddress.Parse("127.0.0.1"), 8118,
                    IPAddress.Parse("127.0.0.1"), 9150,
                    ProxyConfig.SocksVersion.Five
                ));
                using (var ms = new MemoryStream())
                {
                    if (HttpUtility.UrlEncode(phrase).Length < 101)
                    {
                        var request = WebRequest.Create(string.Format(ONLINE_SPANISH_TTS_URL, Uri.EscapeDataString(phrase)));
                        request.Proxy = proxy;
                        using (Stream stream = request.GetResponse().GetResponseStream())
                        {
                            byte[] buffer = new byte[32768];
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                        }

                        return ms.ToArray();
                    }
                    else return null;
                }
            }
            catch { return null; }
        }

        private static byte[] GetEnglishAudioOnline(string phrase)
        {
            try
            {
                var proxy = new SocksWebProxy(new ProxyConfig(
                    IPAddress.Parse("127.0.0.1"), 8118,
                    IPAddress.Parse("127.0.0.1"), 9150,
                    ProxyConfig.SocksVersion.Five
                ));
                using (var ms = new MemoryStream())
                {
                    if (HttpUtility.UrlEncode(phrase).Length < 101)
                    {
                        var request = WebRequest.Create(string.Format(ONLINE_ENGLISH_TTS_URL, Uri.EscapeDataString(phrase.Replace("'",""))));
                        request.Proxy = proxy;
                        using (Stream stream = request.GetResponse().GetResponseStream())
                        {
                            byte[] buffer = new byte[32768];
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }
                        }

                        return ms.ToArray();
                    }
                    else return null;
                }
            }
            catch { return null; }
        }
    }
}
