using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace P4PSpeechDB
{
    public class MainWindowViewModel : ObservableObject
    {
        ObservableCollection<ProjectViewModel> projects = new ObservableCollection<ProjectViewModel>();
        ObservableCollection<SpeakerViewModel> speakers = new ObservableCollection<SpeakerViewModel>();
        DataGridLoader dgl = new DataGridLoader();


        private ProjectViewModel selectedProject;

        public MainWindowViewModel()
        {
            projects = dgl.getProjects();
        }

        public ObservableCollection<ProjectViewModel> Projects
        {
            get
            {
                return projects;
            }
            set
            {
                projects = value;
            }
        }

        public ObservableCollection<SpeakerViewModel> Speakers
        {
            get
            {
                return speakers;
            }
            set
            {
                speakers = value;
            }
        }

        public ProjectViewModel SelectedProject
        {
            get { return selectedProject; }
            set { selectedProject = value; RaisePropertyChanged("SelectedProject"); }
        }

        void ProjectSelectedExecute()
        {

            if (!(SelectedProject is ProjectViewModel))
                return;

            speakers = dgl.getSpeakers(SelectedProject.PID);

            RaisePropertyChanged("Speakers");
            System.Diagnostics.Debug.WriteLine(speakers.Count);
        }


        public ICommand ProjectSelected { get { return new RelayCommand(ProjectSelectedExecute); } }
        //public ICommand ProjectSelected { get { return new RelayCommand<object>((s) => ProjectSelectedExecute(s)); } }
    }
}
