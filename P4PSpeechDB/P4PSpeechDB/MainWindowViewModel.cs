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

        #region Members
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

        #endregion

        #region Datagrid operations


        public ICommand ProjectSelected { get { return new RelayCommand(ProjectSelectedExecute); } }

        public ProjectViewModel SelectedProject
        {
            get { System.Console.WriteLine("3"); return selectedProject; }
            set
            {
                selectedProject = value;
            }
        }

        void ProjectSelectedExecute()
        {

            if (!(SelectedProject is ProjectViewModel))
                return;

            speakers = dgl.getSpeakers(SelectedProject.PID);
            RaisePropertyChanged("SpeakersView");
            speakersView = (ListCollectionView)CollectionViewSource.GetDefaultView(speakers);
            
            // Do this only once
            if (speakersView.GroupDescriptions.Count() == 0)
            {
                speakersView.GroupDescriptions.Add(new PropertyGroupDescription("SpeakerName"));
                speakersView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
            }

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
        /*On button click, add speech file. */
        public ICommand AddFiles { get { return new RelayCommand(AddFilesExecute); } }

        void AddFilesExecute()
        {
            moa.AddFiles();
        }

        /*On button click, add analysis file. */
        public ICommand AddAnalysis { get { return new RelayCommand(AddAnalysisExecute); } }

        void AddAnalysisExecute()
        {
            moa.AddAnalysis();
        }

        #endregion

        #region Other operations

        /*On button click, create template file. */
        public ICommand GenerateTemplate { get { return new RelayCommand(GenerateTemplateExecute); } }

        void GenerateTemplateExecute()
        {
            moa.GenerateTemplate();
        }

        #endregion
        #region ComboBox operations

        protected ComboBox m_SortValue;
        private string str_SortValue;

        /// <summary>
        ///  
        /// </summary>
        //public string SortValue
        //{
        //    get { System.Console.WriteLine((string)m_SortValue.SelectedValue); return str_SortValue=(string)m_SortValue.SelectedValue; }
        //    set
        //    {
        //        if (str_SortValue != value)
        //        {
        //            str_SortValue = value;
        //            //RaisePropertyChanged("FirstSelectedValue");
        //        }
        //    }
        //}

        //protected ObservableCollection<string> m_FirstComboValues;

        ///// <summary>
        /////  
        ///// </summary>
        //public ObservableCollection<string> FirstComboValues
        //{
        //    get { return m_FirstComboValues; }
        //    set
        //    {
        //        if (m_FirstComboValues != value)
        //        {
        //            m_FirstComboValues = value;
        //            RaisePropertyChanged("FirstComboValues");
        //        }
        //    }
        //}
        #endregion
        //public ICommand ProjectSelected { get { return new RelayCommand<object>((s) => ProjectSelectedExecute(s)); } }
    }
}
