using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4PSpeechDB
{
    public class ProjectViewModel : ObservableObject
    {
        Project project;
        public ProjectViewModel() { project = new Project(); }

        public Project Project
        {
            get { return project; }
            set { project = value; }
        }

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

    }


}
