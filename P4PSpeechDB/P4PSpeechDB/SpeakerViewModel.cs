using MicroMvvm;
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
    /* ViewModel class for Speaker model. */
    public class SpeakerViewModel : ObservableObject
    {
        Speaker speaker;
        public SpeakerViewModel() { speaker = new Speaker(); }

        public Speaker Speaker
        {
            get { return speaker; }
            set { speaker = value; }
        }

        #region Properties
        public string ID
        {
            get { return Speaker.ID; }
            set
            {
                if (Speaker.ID != value)
                {
                    Speaker.ID = value;
                    RaisePropertyChanged("ID");
                }
            }
        }

        public string Name
        {
            get { return Speaker.Name; }
            set
            {
                if (Speaker.Name != value)
                {
                    Speaker.Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string PID
        {
            get { return Speaker.PID; }
            set
            {
                if (Speaker.PID != value)
                {
                    Speaker.PID = value;
                    RaisePropertyChanged("PID");
                }
            }
        }

        public string FileType
        {
            get { return Speaker.FileType; }
            set
            {
                if (Speaker.FileType != value)
                {
                    Speaker.FileType = value;
                    RaisePropertyChanged("FileType");
                }
            }
        }

        public string SpeakerName
        {
            get { return Speaker.SpeakerName; }
            set
            {
                if (Speaker.SpeakerName != value)
                {
                    Speaker.SpeakerName = value;
                    RaisePropertyChanged("SpeakerName");
                }
            }
        }

        public string Age
        {
            get { return Speaker.Age; }
            set
            {
                if (Speaker.Age != value)
                {
                    Speaker.Age = value;
                    RaisePropertyChanged("Age");
                }
            }
        }
        #endregion

    }


}
