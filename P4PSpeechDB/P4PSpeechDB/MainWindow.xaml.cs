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
        //private string databaseRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles"; //Where the P4Ptestfiles folder is
        private string testDBRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\TestDB";
        private DBConnection conn;
        MySqlConnection myConn = null;
        private List<String> tableNames = new List<String>();
        private ObservableCollection<Row> rowS; //DAtagrid row item
        private ObservableCollection<Row> rowA; //DAtagrid row item
        private ObservableCollection<ProjectViewModel> projects; //DAtagrid row item
        DataGridLoader dgl;
        ProgressBar prog = null;

        public Boolean IsExpanded { get; set; }
        private string groupValue = "Speaker"; //Default grouping on this value


        MoaCore moa;
        MainWindowViewModel vm;

        public MainWindow()
        {

            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;


            vm = (MainWindowViewModel)this.DataContext;

        }


        private void speakerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;
            if (typeItem != null)
            {
                groupValue = typeItem.Name.ToString();
                vm.setGroupMode(groupValue);
            }
        }

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
                    vm.downloadProject(this, parentDir + @"\testOutput\", pr.PID);
                    break;
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    // Open file system to select file(s)
        //    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        //    dlg.Multiselect = true;


        //    Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method 
        //    byte[] rawData;
        //    List<Tuple<string, byte[]>> dataList = new List<Tuple<string, byte[]>>();

        //    // Add all files selected into the the db. If multiple files added, project destination is the same.
        //    foreach (String file in dlg.FileNames)
        //    {
        //        // Get the selected file name and display in a TextBox 
        //        if (result.HasValue == true && result.Value == true)
        //        {
        //            rawData = File.ReadAllBytes(file);
        //            dataList.Add(Tuple.Create(file, rawData));
        //        }
        //    }

        //    AnalysisMsgPrompt a = new AnalysisMsgPrompt(new DataGridLoader(), null);



        //    if (a.ShowDialog() == true)
        //    {
        //        dgl.loadSpeakers(a.PID);
        //        rowS = dgl.getCollection("S");
        //        foreach (var elem in rowS.ToList())
        //            ((dynamic)rowS).Add((Speaker)elem);


        //        using ((myConn = new DBConnection().getConn()))
        //        using (MySqlCommand comm = myConn.CreateCommand())
        //        {
        //            myConn.Open();

        //            foreach (var dataItem in dataList)
        //            {


        //                try
        //                {
        //                    comm.CommandText = "INSERT INTO analysis (AID, File, Description) VALUES(@AID, @FileAsBlob, @Desc)";
        //                    comm.Parameters.AddWithValue("@AID", dataItem.Item1);
        //                    comm.Parameters.AddWithValue("@FileAsBlob", dataItem.Item2);
        //                    if (a.Desc.Equals(""))
        //                    {
        //                        comm.Parameters.AddWithValue("@Desc", "No description");
        //                    }
        //                    else
        //                    {

        //                        comm.Parameters.AddWithValue("@Desc", a.Desc);
        //                    }
        //                    comm.ExecuteNonQuery();

        //                    //Add to the mapping table(to link with speaker)
        //                    List<Row> startsWithAge = rowS.Where(s => ((Speaker)s).SpeakerName.StartsWith(a.Age)).ToList();

        //                    MessageBox.Show(a.Age);
        //                    foreach (var row in rowS)
        //                    {

        //                        //comm.CommandText = "create table if not exists files2analysis (AID varchar(150) primary key, ID varchar(150) primary key)";
        //                        //comm.ExecuteNonQuery();
        //                        if (((Speaker)row).SpeakerName.StartsWith(a.Age))
        //                        {


        //                            comm.CommandText = "INSERT IGNORE INTO files2analysis (ID, AID) VALUES (@ID2, @AID2)";
        //                            comm.Parameters.Clear();
        //                            comm.Parameters.AddWithValue("@ID2", ((Speaker)row).ID);
        //                            comm.Parameters.AddWithValue("@AID2", dataItem.Item1);
        //                            comm.ExecuteNonQuery();
        //                        }

        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    //conn.handleException(e);
        //                }
        //            }

        //        }
        //    }


        //}

        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            testDBRoot = dialog.SelectedPath;
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

        private void speakerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;

            groupValue = typeItem.Name.ToString();
            //buildDatagridGroups(new ListCollectionView(rowS));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProjectViewModel pr = dataGridProjects.SelectedValue as ProjectViewModel;
            MenuItem mi = sender as MenuItem;


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



