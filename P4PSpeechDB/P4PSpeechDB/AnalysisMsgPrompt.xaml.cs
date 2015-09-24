using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

/** Authors: Govindu Samarasinghe, Rodel Rojos
 *  Date: 2015
 * 
 *  Project: The Big Data Speech Processing Platform
 *  Project proposed by the ECE department of The University of Auckland
 */

namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for AnalysisMsgPrompt.xaml
    /// </summary>
    public partial class AnalysisMsgPrompt : Window
    {
        private DataGridLoader dgl;


        public AnalysisMsgPrompt(DataGridLoader dgl,  ObservableCollection<ProjectViewModel> vm)
        {
            InitializeComponent();
            this.dgl = dgl;

            fillCombos(vm);
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);


        }

        // fill in the values of each project name in the combobox
        private void fillCombos(ObservableCollection<ProjectViewModel>  vm)
        {
            var rowPIDList = vm.Select(p => p.PID).ToList();
            cbChooseProject.ItemsSource = rowPIDList;

        }

        public string PID
        {
            get { return this.cbChooseProject.SelectedItem.ToString(); }
            set { }
        }

        public string Age
        {
            get { return this.cbChooseSpeaker.SelectedItem.ToString(); }
            set { }
        }

        public string Desc
        {
            get { return this.txtDesc.Text; }
            set { }
        }


        void PromptDialog_Loaded(object sender, RoutedEventArgs e)
        {
            cbChooseProject.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cbChooseProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            cbChooseSpeaker.IsEnabled = true;
            ComboBox cmb = sender as ComboBox;
            

            //foreach (var elem in dgl.getCollection("S"))
            //    ((ObservableCollection<Speaker>)this.rowS).Add((dynamic)elem);


            var rowSAgeList = dgl.getSpeakers(cmb.SelectedItem.ToString()).Select(s => s.Age).Distinct().ToList();
            cbChooseSpeaker.ItemsSource = rowSAgeList;
            btnAdd.IsEnabled = true;
        }

    }
}
