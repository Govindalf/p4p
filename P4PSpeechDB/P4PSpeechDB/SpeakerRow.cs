using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4PSpeechDB
{

    class SpeakerRow : Row

    {
        public string ID { get; set; }
        public string filePath { get; set; }
        public string ProjectName { get; set; }
        public string tableName { get; set; }
        public string Speaker { get; set; }
        public string Age { get; set; }
    }
}