﻿using MicroMvvm;
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
                moa.openOrPlayFile(SelectedSpeaker);
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
                moa.openOrPlayFile(SelectedAnalysis);
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
        public ICommand GenerateTemplate { get { return new RelayCommand(GenerateTemplateExecute); } }

        void GenerateTemplateExecute()
        {
            moa.GenerateTemplate();
        }

        #endregion
        //public ICommand ProjectSelected { get { return new RelayCommand<object>((s) => ProjectSelectedExecute(s)); } }
    }
}
