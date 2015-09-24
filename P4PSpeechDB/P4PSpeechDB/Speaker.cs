using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/** Authors: Govindu Samarasinghe, Rodel Rojos
 *  Date: 2015
 * 
 *  Project: The Big Data Speech Processing Platform
 *  Project proposed by the ECE department of The University of Auckland
 */
namespace P4PSpeechDB
{

    /* Model class representing a Speaker file. */
    public class Speaker

    {
        public string ID { get; set; }
        public string Name { get; set; }
        //public string FilePath { get; set; }
        public string PID { get; set; }
        public string FileType { get; set; }
        public string SpeakerName { get; set; }
        public string Age { get; set; }
    }
}