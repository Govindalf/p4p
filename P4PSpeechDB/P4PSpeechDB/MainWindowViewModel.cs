using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        private CollectionView speakersView;

        private MoaCore moa;
        private ProjectViewModel selectedProject;
        private SpeakerViewModel selectedSpeaker;

        private ICommand _groupColumn;
  

        public MainWindowViewModel()
        {
            moa = new MoaCore(this, dgl);
            projects = dgl.getProjects();

            speakersView = (ListCollectionView)CollectionViewSource.GetDefaultView(this.Speakers);
            //cv.GroupDescriptions.Add(new PropertyGroupDescription("Speakers.Name"));
            //.ItemsSource = cv;
            //ICollectionView speak = CollectionViewSource.GetDefaultView(speakers);
            //speak.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
        }

        public CollectionView SpeakersView
        {
            get
            {
                return speakersView;
            }
            set
            {
                speakersView = value;
                RaisePropertyChanged("SpeakersView");
            }
        }

        public ICommand GroupColumn
        {
            get
            {
                if (_groupColumn == null)
                    _groupColumn = new RelayCommand<object>(
                        (param) =>
                        {

                            string header = param as string;
                            SpeakersView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
                            //SpeakersView.SortDescriptions.Add(new SortDescription("TargetProperty.Name", ListSortDirection.Ascending));
                        });

                return _groupColumn;
            }
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
            set
            {
                selectedProject = value; RaisePropertyChanged("SpeakersView");
            }
        }

        void ProjectSelectedExecute()
        {

            if (!(SelectedProject is ProjectViewModel))
                return;

            speakers = dgl.getSpeakers(SelectedProject.PID);

            RaisePropertyChanged("SpeakersView");
            System.Console.WriteLine("Project Selected");
            speakersView = (ListCollectionView)CollectionViewSource.GetDefaultView(this.Speakers);
            // do this only once or reset the collectionView.
            speakersView.GroupDescriptions.Add(new PropertyGroupDescription("SpeakerName"));
            speakersView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));

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

        #region Database operations
        public ICommand AddFiles { get { return new RelayCommand(AddFilesExecute); } }

        void AddFilesExecute()
        {
            moa.AddFiles();
        }

        public ICommand AddAnalysis { get { return new RelayCommand(AddAnalysisExecute); } }

        void AddAnalysisExecute()
        {
            moa.AddAnalysis();
        }

        #endregion
        //public ICommand ProjectSelected { get { return new RelayCommand<object>((s) => ProjectSelectedExecute(s)); } }
    }
}
