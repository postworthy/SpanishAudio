using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpanishAudio.Models
{
    [Serializable]
    public class Phrase
    {
        public string SpanishKeyword { get; set; }
        public string Spanish { get; set; }
        public bool AllowModifySpanish { get; set; }
        public byte[] SpanishAudio { get; set; }
        public string English { get; set; }
        public bool AllowModifyEnglish { get; set; }
        public byte[] EnglishAudio { get; set; }

    }
}
