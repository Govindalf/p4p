using MicroMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P4PSpeechDB
{
    public class MainWindowViewModel
    {
        ObservableCollection<ProjectViewModel> projects = new ObservableCollection<ProjectViewModel>();

        public MainWindowViewModel()
        {
            var dgl = new DataGridLoader();
            projects = dgl.getProjects;
            System.Diagnostics.Debug.WriteLine(projects.Count.ToString());
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
    }
}
