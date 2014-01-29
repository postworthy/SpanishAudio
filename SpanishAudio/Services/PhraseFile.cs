using NAudio.Wave;
using SoundTouchNet;
using SpanishAudio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SpanishAudio.Services
{
    public static class PhraseFile
    {
        private const string BASE_PATH = "phrases/";
        private const string BASE_PATH_MP3 = "phrases/mp3/";
        private const string BASE_PATH_MP3_SLOW = "phrases/mp3/slow/";
        private const string BASE_PATH_MP3_MASTER = "phrases/mp3/master/";
        private const string BASE_PATH_MP3_DYNAMIC = "phrases/mp3/dynamic/";

        public static void SavePhrases(string keyword, IEnumerable<Phrase> phrases)
        {
            if (phrases.Count() > 0)
            {
                var file = BASE_PATH + keyword + ".phrases";
                File.Delete(file);

                (new FileInfo(BASE_PATH)).Directory.Create();

                MemoryStream m = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(m, phrases.ToList());

                File.WriteAllBytes(file, m.ToArray());
            }
        }

        public static void SaveAudio(string keyword, List<Phrase> phrases, bool overwrite = false)
        {
            if (phrases.Count() > 0)
            {
                (new FileInfo(BASE_PATH)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3)).Directory.Create();

                for (int i = 0; i < phrases.Count; i++)
                {
                    var file = BASE_PATH_MP3 + keyword + "_" + (i + 1) + ".mp3";
                    if (overwrite || !File.Exists(file))
                    {
                        File.Delete(file);
                        var phrase = phrases[i];
                        if (phrase.SpanishAudio != null && phrase.EnglishAudio != null)
                            Concatenate(file, getPhraseArray(phrase));
                    }
                }
            }
        }

        private static void SaveAudioSlowSpanish(string keyword, List<Phrase> phrases)
        {
            if (phrases.Count() > 0)
            {
                (new FileInfo(BASE_PATH)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3_SLOW)).Directory.Create();

                for (int i = 0; i < phrases.Count; i++)
                {
                    var spanish = BASE_PATH_MP3_SLOW + keyword + "_" + (i + 1) + "_slow_es.mp3";

                    File.Delete(spanish);

                    var phrase = phrases[i];
                    if (phrase.SpanishAudio != null) File.WriteAllBytes(spanish, SlowAudio(phrase.SpanishAudio));
                }
            }
        }

        public static void SaveAudioMaster(string keyword, List<Phrase> phrases)
        {
            if (phrases.Count() > 0)
            {
                (new FileInfo(BASE_PATH)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3_MASTER)).Directory.Create();
                var file = BASE_PATH_MP3_MASTER + keyword + ".mp3";
                if (!File.Exists(file))
                {
                    var dataFiles = phrases
                            .Where(phrase => phrase.EnglishAudio != null && phrase.SpanishAudio != null)
                            .SelectMany(phrase => getPhraseArray(phrase)).ToList();
                    if (dataFiles.Count > 0)
                        Concatenate(file, dataFiles);
                }
            }
        }

        public static void SaveDynamicAudio(int level, List<Phrase> phrases)
        {
            if (phrases.Count() > 0)
            {
                (new FileInfo(BASE_PATH)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3)).Directory.Create();
                (new FileInfo(BASE_PATH_MP3_DYNAMIC)).Directory.Create();
                var file = BASE_PATH_MP3_DYNAMIC + "Dynamic Level " + level + ".mp3";
                if (!File.Exists(file))
                {
                    var dataFiles = phrases
                            .Where(phrase => phrase.EnglishAudio != null && phrase.SpanishAudio != null)
                            .SelectMany(phrase => getPhraseArray(phrase)).ToList();
                    if (dataFiles.Count > 0)
                        Concatenate(file, dataFiles);
                }
            }
        }

        private static byte[][] getPhraseArray(Phrase phrase)
        {
            return new byte[][] { phrase.EnglishAudio, phrase.SpanishAudio, phrase.EnglishAudio, phrase.SpanishAudio, phrase.SpanishAudio };
        }

        public static IEnumerable<Phrase> GetPhrases(string keyword)
        {
            if (Exists(keyword))
            {
                MemoryStream m = new MemoryStream(File.ReadAllBytes(BASE_PATH + keyword + ".phrases"));
                var formatter = new BinaryFormatter();
                return (IEnumerable<Phrase>)formatter.Deserialize(m);
            }
            return null;
        }

        public static bool Exists(string keywords)
        {
            return File.Exists(BASE_PATH + keywords + ".phrases");
        }

        private static void Concatenate(string outputFile, IEnumerable<byte[]> mp3s)
        {
            int i = 0;
            byte[] buffer = new byte[1024];
            NAudio.Lame.LameMP3FileWriter mp3FileWriter = null;
            var format = new WaveFormat();
            byte[][] silence = new byte[][] { new byte[1], new byte[1], new byte[1] };

            try
            {

                mp3FileWriter = new NAudio.Lame.LameMP3FileWriter(outputFile, format, 128);
                silence = new byte[][] { 
                    new byte[(format.AverageBytesPerSecond * 1)],
                    new byte[(format.AverageBytesPerSecond * 2)],
                    new byte[(format.AverageBytesPerSecond * 3)],
                };
                mp3FileWriter.Write(silence[1], 0, silence[1].Length);


                foreach (byte[] mp3 in mp3s)
                {
                    i++;

                    File.Delete("temp.wav");
                    File.Delete("temp2.wav");

                    using (var mp3Stream = new MemoryStream(mp3))
                    using (Mp3FileReader reader = new Mp3FileReader(mp3Stream))
                    {
                        WaveFileWriter.CreateWaveFile("temp.wav", reader);
                    }

                    using (WaveFileReader reader = new WaveFileReader("temp.wav"))
                    {
                        if (!reader.WaveFormat.Equals(format))
                        {
                            using (var conversionStream = new WaveFormatConversionStream(format, reader))
                            {
                                WaveFileWriter.CreateWaveFile("temp2.wav", conversionStream);
                            }
                            using (WaveFileReader reader2 = new WaveFileReader("temp2.wav"))
                            {
                                int read;
                                while ((read = reader2.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    mp3FileWriter.Write(buffer, 0, read);
                                }
                            }
                        }
                        else
                        {
                            int read;
                            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                mp3FileWriter.Write(buffer, 0, read);
                            }
                        }
                    }
                    mp3FileWriter.Write(silence[1], 0, silence[1].Length);
                }
                mp3FileWriter.Flush();
            }
            finally
            {
                File.Delete("temp.wav");
                File.Delete("temp2.wav");
                if (mp3FileWriter != null)
                {
                    mp3FileWriter.Dispose();
                }
            }
        }

        private static byte[] SlowAudio(byte[] mp3, float newTempo = 0.95f, float newPitch = 1.2f, float newRate = .8f)
        {
            const int BUFFER_SIZE = 1024 * 16;
            byte[] buffer = new byte[BUFFER_SIZE];
            var format = new WaveFormat();

            try
            {
                using (var output = new MemoryStream())
                {

                    File.Delete("temp.wav");
                    File.Delete("temp2.wav");

                    using (var mp3Stream = new MemoryStream(mp3))
                    using (Mp3FileReader reader = new Mp3FileReader(mp3Stream))
                    {
                        WaveFileWriter.CreateWaveFile("temp.wav", reader);
                    }

                    using (WaveFileReader reader = new WaveFileReader("temp.wav"))
                    {
                        int numChannels = reader.WaveFormat.Channels;

                        if (numChannels > 2)
                            throw new Exception("SoundTouch supports only mono or stereo.");

                        int sampleRate = reader.WaveFormat.SampleRate;

                        int bitPerSample = reader.WaveFormat.BitsPerSample;

                        SoundStretcher stretcher = new SoundStretcher(sampleRate, numChannels);
                        var writer = new NAudio.Lame.LameMP3FileWriter(output, new WaveFormat(sampleRate, 16, numChannels), 128);

                        stretcher.Tempo = newTempo;
                        stretcher.Pitch = newPitch;
                        stretcher.Rate = newRate;


                        short[] buffer2 = null;

                        if (bitPerSample != 16 && bitPerSample != 8)
                        {
                            throw new Exception("Not implemented yet.");
                        }

                        if (bitPerSample == 8)
                        {
                            buffer2 = new short[BUFFER_SIZE];
                        }

                        bool finished = false;

                        while (true)
                        {
                            int bytesRead = 0;
                            if (!finished)
                            {
                                bytesRead = reader.Read(buffer, 0, BUFFER_SIZE);

                                if (bytesRead == 0)
                                {
                                    finished = true;
                                    stretcher.Flush();
                                }
                                else
                                {
                                    if (bitPerSample == 16)
                                    {
                                        stretcher.PutSamplesFromBuffer(buffer, 0, bytesRead);
                                    }
                                    else if (bitPerSample == 8)
                                    {
                                        for (int i = 0; i < BUFFER_SIZE; i++)
                                            buffer2[i] = (short)((buffer[i] - 128) * 256);
                                        stretcher.PutSamples(buffer2);
                                    }
                                }
                            }
                            bytesRead = stretcher.ReceiveSamplesToBuffer(buffer, 0, BUFFER_SIZE);
                            writer.Write(buffer, 0, bytesRead);

                            if (finished && bytesRead == 0)
                                break;
                        }
                        reader.Close();
                        writer.Close();
                    }

                    return output.ToArray();
                }
            }
            finally
            {
                File.Delete("temp.wav");
                File.Delete("temp2.wav");
            }

            /////////////////////////////


        }
    }
}
