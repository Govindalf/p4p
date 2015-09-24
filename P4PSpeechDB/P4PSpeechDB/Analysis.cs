using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4PSpeechDB
{
    public class Analysis : Row
    {
        public string AID { get; set; }
        public string Description { get; set; }
        public string FileData { get; set; }
        public string FileType { get; set; }
    }
}