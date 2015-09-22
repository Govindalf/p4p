using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace P4PSpeechDB
{
    public class MainWindowViewModel : ObservableObject
    {
        ObservableCollection<ProjectViewModel> projects = new ObservableCollection<ProjectViewModel>();
        ObservableCollection<SpeakerViewModel> speakers = new ObservableCollection<SpeakerViewModel>();
        ObservableCollection<AnalysisViewModel> analyses = new ObservableCollection<AnalysisViewModel>();
        DataGridLoader dgl = new DataGridLoader();


        private ProjectViewModel selectedProject;
        private SpeakerViewModel selectedSpeaker;

        public MainWindowViewModel()
        {
            projects = dgl.getProjects();


            ICollectionView speak = CollectionViewSource.GetDefaultView(speakers);
            speak.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
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

        public ObservableCollection<AnalysisViewModel> Analysis
        {
            get
            {
                return analyses;
            }
            set
            {
                analyses = value;
            }
        }

        #region Datagrid operations


        public ICommand ProjectSelected { get { return new RelayCommand(ProjectSelectedExecute); } }

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
        }


        public ICommand SpeakerSelected { get { return new RelayCommand(SpeakerSelectedExecute); } }

        public SpeakerViewModel SelectedSpeaker
        {
            get { return selectedSpeaker; }
            set { selectedSpeaker = value; RaisePropertyChanged("SelectedSpeaker"); }
        }

        void SpeakerSelectedExecute()
        {

            if (!(SelectedSpeaker is SpeakerViewModel))
                return;

            analyses = dgl.getAnalysis(SelectedSpeaker.Name);

            RaisePropertyChanged("Analysis");
        }
        #endregion


        //public ICommand ProjectSelected { get { return new RelayCommand<object>((s) => ProjectSelectedExecute(s)); } }
    }
}
