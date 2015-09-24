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
    /* ViewModel class for Analysis model. */
    public class AnalysisViewModel : ObservableObject
    {
        Analysis analysis;
        public AnalysisViewModel() { analysis = new Analysis(); }

        public Analysis Analysis
        {
            get { return analysis; }
            set { analysis = value; }
        }

        #region Properties
        public string AID
        {
            get { return Analysis.AID; }
            set
            {
                if (Analysis.AID != value)
                {
                    Analysis.AID = value;
                    RaisePropertyChanged("AID");
                }
            }
        }

        public string Description
        {
            get { return Analysis.Description; }
            set
            {
                if (Analysis.Description != value)
                {
                    Analysis.Description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }

        public string FileData
        {
            get { return Analysis.FileData; }
            set
            {
                if (Analysis.FileData != value)
                {
                    Analysis.FileData = value;
                    RaisePropertyChanged("FileData");
                }
            }
        }

        public string FileType
        {
            get { return Analysis.FileType; }
            set
            {
                if (Analysis.FileType != value)
                {
                    Analysis.FileType = value;
                    RaisePropertyChanged("FileType");
                }
            }
        }

        #endregion

    }


}
