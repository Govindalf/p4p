using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4PSpeechDB
{
    public class Project : Row
    {
        public string PID { get; set; }
        public string DateCreated { get; set; }
        public string Description { get; set; }

    }
}