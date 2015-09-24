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

    /* ViewModel class for Project model. */
    public class ProjectViewModel : ObservableObject
    {
        Project project;
        public ProjectViewModel() { project = new Project(); }

        public Project Project
        {
            get { return project; }
            set { project = value; }
        }

        #region Properties
        public string PID
        {
            get { return Project.PID; }
            set
            {
                if (Project.PID != value)
                {
                    Project.PID = value;
                    RaisePropertyChanged("PID");
                }
            }
        }

        public string DateCreated
        {
            get { return Project.DateCreated; }
            set
            {
                if (Project.DateCreated != value)
                {
                    Project.DateCreated = value;
                    RaisePropertyChanged("DateCreated");
                }
            }
        }

        public string Description
        {
            get { return Project.Description; }
            set
            {
                if (Project.Description != value)
                {
                    Project.Description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }
        #endregion

    }


}
