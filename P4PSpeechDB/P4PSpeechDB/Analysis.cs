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

    /* Model class representing a Analysis file. */
    public class Analysis
    {
        public string AID { get; set; }
        public string Description { get; set; }
        public string FileData { get; set; }
        public string FileType { get; set; }
    }
}