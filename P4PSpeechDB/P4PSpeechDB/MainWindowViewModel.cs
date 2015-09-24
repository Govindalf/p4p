using MicroMvvm;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool isExpanded;
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

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { isExpanded = value; RaisePropertyChanged("IsExpanded"); }
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
        public ICommand ProjectSelected { get { return new RelayCommand(ProjectSelectedExecute); } }

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
            get { return selectedProject; }
            set
            {
                selectedProject = value; RaisePropertyChanged("SpeakersView"); 
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

        /*Sets up custom datagrid grouping when combo option selected. */
        public void setGroupMode(string groupValue)
        {
            SpeakersView.GroupDescriptions.Clear();
            SpeakersView.GroupDescriptions.Add(new PropertyGroupDescription(groupValue));

            if (!groupValue.Equals("FileType"))
                SpeakersView.GroupDescriptions.Add(new PropertyGroupDescription("Name"));
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

        /*On button click, deletes selected files. Takes in both selected items in speaker and analysis grids respectively.*/
        public void DeleteFiles(IList listOfSGridItems, IList listOfAGridItems)
        {
            //Check which datagrid item is actually selected currently, then delete from that.
            //if(SelectedSpeaker == null)
            //    moa.DeleteFiles(listOfAGridItems);
            //else if (SelectedAnalysis == null)
            moa.DeleteFiles(listOfSGridItems);

        }


        #endregion

        #region Other operations

        /*On button click, changes default path where the database is stored on disk. */
        public ICommand PathSettings { get { return new RelayCommand(PathSettingsExecute); } }

        void PathSettingsExecute()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            moa.RootFolder = dialog.SelectedPath;

        }

        /*On button click, creates a template file. */
        public ICommand GenerateTemplate { get { return new RelayCommand(GenerateTemplateExecute); } }
        void GenerateTemplateExecute()
        {
            moa.GenerateTemplate();
        }

        private string searchText;
        /*Searching the datagrid */
        public void SearchGrid(string searchText)
        {
            //Eh well
            this.searchText = searchText;

            SpeakersView.Filter += new Predicate<object>(searchFilter);

        }


        /*Regex filter used for search.*/
        public bool searchFilter(object item)
        {
            var obj = item as SpeakerViewModel;

            return (Regex.IsMatch(obj.Name, searchText, RegexOptions.IgnoreCase));


        }
        #endregion
        #region ComboBox operations

        protected string m_SortValue;
        //private string str_SortValue;

        /// <summary>
        ///  
        /// </summary>
        public string SortValue
        {
            get { return m_SortValue; }
            set
            {
                if (m_SortValue != value)
                {
                    m_SortValue = value;
                    //RaisePropertyChanged("FirstSelectedValue");
                }
            }
        }

        protected ObservableCollection<string> m_SortValues;

        /// <summary>
        ///  
        /// </summary>
        public ObservableCollection<string> SortSpeakers
        {
            get { return m_SortValues; }
            set
            {
                if (m_SortValues != value)
                {
                    m_SortValues = value;
                    RaisePropertyChanged("SortSpeakers");
                }
            }
        }
        #endregion

        #region Multithreaded tasks

        /* Downloads a selected project on a background thread. */
        private void backgroundWorker_DoWork(
            object sender,
            DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            string PID = parameters[1] as string;
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
        public void downloadProject(MainWindow view, string PID)
        {

            BackgroundWorker backgroundWorker;

            // Instantiate BackgroundWorker and attach handlers to its 
            // DowWork and RunWorkerCompleted events.
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);

            object[] parameters = new object[] { view, PID };

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

        /* Uploads a selected project on a background thread. */
        private void backgroundWorker_DoUploadWork(
            object sender,
            DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            var dispatcher = parameters[0] as Dispatcher;
            var selectedPath = parameters[1] as string;

            List<String> projectDetails = null;
            dispatcher.Invoke((Action)(() =>
            {
                projectDetails = moa.GetFolderName();
                prog.Show();
            }));

            moa.AddFolders(projectDetails, selectedPath);



        }


        /* Uploads projects on a different thread to UI thread, so user can do other tasks while uploading. */
        public void uploadProject(Dispatcher dispatcher, string selectedPath)
        {

            BackgroundWorker backgroundWorker;

            // Instantiate BackgroundWorker and attach handlers to its 
            // DowWork and RunWorkerCompleted events.
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoUploadWork);
            backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);

            object[] parameters = new object[] { dispatcher, selectedPath };

            prog = new ProgressBar();
            // Start the download operation in the background. 
            backgroundWorker.RunWorkerAsync();

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

        #endregion
    }
}
