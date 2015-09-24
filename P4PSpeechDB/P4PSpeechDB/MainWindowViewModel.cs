using MicroMvvm;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

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
        private AnalysisViewModel selectedAnalysis;

        private ICommand _groupColumn;
        ProgressBar prog = null;

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

        /*Runs when project selected in the projects datagrid.*/
        public ICommand ProjectSelected { get { System.Console.WriteLine("4"); return new RelayCommand(ProjectSelectedExecute); } }
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

        /*The currently selected project object. */
        public ProjectViewModel SelectedProject
        {
            get { System.Console.WriteLine("3"); return selectedProject; }
            set
            {
                selectedProject = value;
            }
        }


        /*The currently selected datagrid speaker item. */
        public SpeakerViewModel SelectedSpeaker
        {
            get { return selectedSpeaker; }
            set { selectedSpeaker = value; RaisePropertyChanged("SelectedSpeaker"); }
        }


        public ICommand SpeakerSelected { get { return new RelayCommand(SpeakerSelectedExecute); } }
        void SpeakerSelectedExecute()
        {

            if (!(SelectedSpeaker is SpeakerViewModel))
                return;

            analyses = dgl.getAnalysis(SelectedSpeaker.Name);

            RaisePropertyChanged("Analysis");
        }

        /*Run this on double clicking speaker datagrid object*/
        public ICommand DoubleClickSpeaker { get { return new RelayCommand(DoubleClickSpeakerExecute); } }
        void DoubleClickSpeakerExecute()
        {

            if (!(SelectedSpeaker is SpeakerViewModel))
                return;

            try
            {
                moa.OpenOrPlayFile(SelectedSpeaker);
            }
            catch
            {
                MessageBox.Show("Access denied, please run application with administrator privileges.");
            }

        }

        /*The currently selected datagrid analysis item. */
        public AnalysisViewModel SelectedAnalysis
        {
            get { return selectedAnalysis; }
            set { selectedAnalysis = value; RaisePropertyChanged("SelectedAnalysis"); }
        }

        /*Run this on double clicking analysis datagrid object*/
        public ICommand DoubleClickAnalysis { get { return new RelayCommand(DoubleClickAnalysisExecute); } }
        void DoubleClickAnalysisExecute()
        {

            if (!(SelectedAnalysis is AnalysisViewModel))
                return;

            try
            {
                moa.OpenOrPlayFile(SelectedAnalysis);
            }
            catch
            {
                MessageBox.Show("Access denied, please run application with administrator privileges.");
            }

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
        public ICommand PathSettings { get { return new RelayCommand(PathSettingsExecute); } }

        void PathSettingsExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            moa.RootFolder = dialog.SelectedPath;
        
        }

                public ICommand GenerateTemplate { get { return new RelayCommand(GenerateTemplateExecute); } }

        void GenerateTemplateExecute()
        {
            moa.GenerateTemplate();
        }



        #endregion


        /* Downloads a selected project on a background thread. */
        private void backgroundWorker_DoWork(
            object sender,
            DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            string PID = parameters[2] as string;
            string path = parameters[1] as string;
            MainWindow view = parameters[0] as MainWindow;

            view.Dispatcher.Invoke((Action)(() =>
            {
                prog.Show();
            }));

            moa.DownloadProject(parameters);

           
        }


        /* Called when the background thread completes. */
        private void backgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            prog.Close();
        }

        /* Downloads projects on a different thread to UI thread, so user can do other tasks while downloading. */
        public void downloadProject(MainWindow view, string path, string PID)
        {

            BackgroundWorker backgroundWorker;

            // Instantiate BackgroundWorker and attach handlers to its 
            // DowWork and RunWorkerCompleted events.
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);

            object[] parameters = new object[] { view, path, PID };

            prog = new ProgressBar();
            // Start the download operation in the background. 
            backgroundWorker.RunWorkerAsync(parameters);

            // Once you have started the background thread you  
            // can exit the handler and the application will  
            // wait until the RunWorkerCompleted event is raised. 

            // Or if you want to do something else in the main thread, 
            // such as update a progress bar, you can do so in a loop  
            // while checking IsBusy to see if the background task is 
            // still running. 

            while (backgroundWorker.IsBusy)
            {

                // Keep UI messages moving, so the form remains  
                // responsive during the asynchronous operation.

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                      new Action(delegate { }));
            }
        }
        //public ICommand ProjectSelected { get { return new RelayCommand<object>((s) => ProjectSelectedExecute(s)); } }
    }
}
