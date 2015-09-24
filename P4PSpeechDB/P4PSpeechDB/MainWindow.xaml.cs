using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;
using Path = System.IO.Path;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using System.Data.SqlClient;

namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Boolean IsExpanded { get; set; }
        MainWindowViewModel vm;

        public MainWindow()
        {

            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;


            vm = (MainWindowViewModel)this.DataContext;

        }

        /*When a combo box item is selected (for grouping datagrid).*/
        private void speakerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;

            if (typeItem != null)
            {
                var groupValue = typeItem.Name.ToString();
                vm.setGroupMode(groupValue);
            }
        }

        /*Context menu when right clicking the projects grid item.*/
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProjectViewModel pr = dataGridProjects.SelectedValue as ProjectViewModel;
            MenuItem mi = sender as MenuItem;

            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string parentDir = Directory.GetParent(folder).Parent.Parent.Parent.FullName;

            //Open the project folder in explorer
            switch (mi.Name)
            {
                case "menuShow":
                    Process.Start("explorer.exe", parentDir + @"\testOutput\" + pr.PID);
                    break;

                case "menuDownload":
                    vm.downloadProject(this, pr.PID);
                    break;
            }
        }

        /*Search the datagrid and filter using a regex.*/
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                vm.SearchGrid(searchBox.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }


        /*Deletes the selected files. */
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            vm.DeleteFiles(dataGridSpeakers.SelectedItems, dataGridAnalysis.SelectedItems);


            ////Reload grid (DOES SAVE EXPANSION STATE)
            //dgl.loadSpeakers((listOfItems[1] as SpeakerRow).PID);
            //dataGridFiles.ItemsSource = new ListCollectionView(dgl.getCollection("S"));
            //buildDatagridGroups(new ListCollectionView(dgl.getCollection("S")));

        }

        /*Adds multiple folders to the database. */
        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (!result.ToString().Equals("Cancel"))
            {
                /*Passes in a dispatcher object, needed for the multithreaded uploads, and the path selected to upload to. */
                vm.uploadProject(this.Dispatcher, dialog.SelectedPath);
            }
        }

        private void ButtonWebMaus_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}



