using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4PSpeechDB
{

    public class SpeakerRow : Row

    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string filePath { get; set; }
        public string PID { get; set; }
        public string FileType { get; set; }
        public string Speaker { get; set; }
        public string Age { get; set; }
    }
}