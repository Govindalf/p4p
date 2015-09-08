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
        private ObservableCollection<Row> rowP; //DAtagrid row item
        DataGridLoader dgl;
        ProgressBar prog = null;

        public Boolean IsExpanded { get; set; }
        private string groupValue = "Speaker"; //Default grouping on this value

        public MainWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnApplicationExit);
            IsExpanded = false;
            this.DataContext = this;
            conn = new DBConnection();

            //Loads all datagrid with relevant data
            dgl = new DataGridLoader(conn, tableNames);
            dgl.setUpDataGrids();
            rowS = dgl.getCollection("S");
            rowP = dgl.getCollection("P");

            InitializeComponent();
            this.speakerCombo.Text = groupValue;

            new DBConnection().createDB();

            try
            {
                // store all of the tables in the mysql database into a list
                using ((myConn = new DBConnection().getConn()))
                using (MySqlCommand comm = myConn.CreateCommand())
                {
                    myConn.Open();
                    comm.CommandText = "show tables from SpeechDB";
                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tableNames.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            //randomlyMatchAnalysis();

            buildDatagridGroups(new ListCollectionView(rowS));

            dataGridProjects.ItemsSource = rowP;

        }


        private void moveFile(string path, string path2)
        {

            // Move the file.
            File.Copy(path, path2, false); //false = dont overwrite
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Open file system to select file(s)
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;

            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".png";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method 
            List<string> folderDetails = new List<string>();
            if (result.HasValue == true && result.Value == true)
            {
                folderDetails = getFolderName(); // only prompt for folder once always
            }
            byte[] rawData;

            // Add all files selected into the the db. If multiple files added, project destination is the same.
            foreach (String file in dlg.FileNames)
            {
                // Get the selected file name and display in a TextBox 
                if (result.HasValue == true && result.Value == true)
                {
                    rawData = File.ReadAllBytes(file);
                    // Open document 
                    string filename = file;
                    string ext = Path.GetExtension(file);

                    //Stores file in appropriate place in file system
                    //moveFile(filename, databaseRoot  /* + WHATEVER THE NEW LOCATION IS ASK CATH */);

                    // Connect to the mysql db if possible
                    using ((myConn = new DBConnection().getConn()))
                    using (MySqlCommand comm = myConn.CreateCommand())
                    {
                        myConn.Open();
                        //try
                        //{
                        //    comm.CommandText = "create table if not exists " + ext.Substring(1) + "(ID varchar(150) primary key, File mediumblob, Speaker varchar(20), ProjectName varchar(100))";
                        //    comm.ExecuteNonQuery();
                        //}
                        //catch (Exception ex)
                        //{
                        //    MessageBox.Show(ex.Message);

                        //}
                        executeInsert(filename, ext, dlg, folderDetails, rawData);

                    }
                }
            }
            dgl.loadProjects();

            //dgl.setUpDataGrids();
        }

        private List<string> getFolderName()
        {
            return FolderMsgPrompt.Prompt("Create new folder", "Folder options", inputType: FolderMsgPrompt.InputType.Text);

        }

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg, List<string> folderDetails, byte[] rawData)
        {

            filename = Path.GetFileName(filename);
            string speaker = Path.GetFileNameWithoutExtension(dlg.SafeFileName).Substring(0, 4);

            using ((myConn = new DBConnection().getConn()))
            using (MySqlCommand comm = myConn.CreateCommand())
            {
                myConn.Open();

                try
                {
                    comm.CommandText = "create table if not exists " + ext.Substring(1) + "(ID varchar(150) primary key, File mediumblob, Speaker varchar(20), ProjectName varchar(100))";
                    comm.ExecuteNonQuery();

                    comm.CommandText = "INSERT INTO " + ext.Substring(1) + "(ID, File, Speaker, ProjectName) VALUES(@ID, @FileAsBlob, @Speaker, @ProjectName)";
                    comm.Parameters.AddWithValue("@ID", filename);
                    comm.Parameters.AddWithValue("@FileAsBlob", rawData);
                    comm.Parameters.AddWithValue("@Speaker", speaker);
                    comm.Parameters.AddWithValue("@ProjectName", folderDetails.First());
                    comm.ExecuteNonQuery();

                    if (folderDetails != null)
                    {
                        comm.CommandText = "INSERT IGNORE INTO projects(PID, dateCreated, description) VALUES(@PID, @dateCreated, @description)";
                        comm.Parameters.AddWithValue("@PID", folderDetails.First());
                        comm.Parameters.AddWithValue("@dateCreated", DateTime.Now.ToString());
                        if (folderDetails.Count == 2)
                        {
                            comm.Parameters.AddWithValue("@description", folderDetails.Last());
                        }
                        else
                        {
                            comm.Parameters.AddWithValue("@description", "No description given");

                        }
                        comm.ExecuteNonQuery();
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


        }

        //Sets up the grouping for the datagrid
        private void buildDatagridGroups(ICollectionView collection)
        {
            PropertyGroupDescription propertyDes = new PropertyGroupDescription("ProjectName");
            collection.GroupDescriptions.Add(new PropertyGroupDescription(groupValue));


            if (groupValue.Equals("Age"))
            {

                collection.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));
            }


            dataGridFiles.ItemsSource = collection;
            dataGridFiles.Items.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
        }


        //When a row in the projects grid is selected

        private void dataGridProjects_GotCellFocus(object sender, RoutedEventArgs e)
        {
            this.emptyGrid.Visibility = System.Windows.Visibility.Hidden;

            if (e.OriginalSource.GetType() == typeof(DataGridCell) && sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as ProjectRow;

                if (item != null)
                {
                    string projectName = item.PID.ToString();
                    dgl.loadSpeakers(projectName);
                    rowS = dgl.getCollection("S");

                    buildDatagridGroups(new ListCollectionView(rowS));
                }

            }
        }

        //When a row in the analysis grid is selected
        private void dataGridFiles_GotCellFocus(object sender, RoutedEventArgs e)
        {
            this.emptyGrid2.Visibility = System.Windows.Visibility.Hidden;

            if (e.OriginalSource.GetType() == typeof(DataGridCell) && sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as SpeakerRow;


                if (item != null)
                {
                    string ID = item.ID;
                    dgl.loadAnalysis(ID);
                    rowA = dgl.getCollection("A");
                    dataGridAnalysis.ItemsSource = new ListCollectionView(rowA);
                    //buildDatagridGroups(new ListCollectionView(rowA));
                }
            }
        }


        private void analysisDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as AnalysisRow;


                if (item != null)
                {
                    string fileType;
                    if (item.FileType == null)
                    {
                        fileType = "txt"; //default in case of shenanigans
                    }
                    else
                    {
                        fileType = item.FileType.ToString();
                    }
                    string fileName = item.AID.ToString();



                    //Check if file exists locally, if not open from db
                    if (File.Exists("..\\..\\..\\..\\testOutput\\ANALYSIS\\" + fileName + "." + fileType))
                    {
                        openOrPlayLocalFile("..\\..\\..\\..\\testOutput\\ANALYSIS\\" + fileName + "." + fileType);
                    }
                    else
                    {
                        using ((myConn = new DBConnection().getConn()))
                        using (MySqlCommand cmd = myConn.CreateCommand())
                        {
                            try
                            {
                                myConn.Open();
                                cmd.CommandText = "SELECT File FROM analysis where AID = '" + fileName + "'";

                                openOrPlayFile(cmd, fileName, fileType, "ANALYSIS", item);

                            }
                            catch (MySqlException ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }

                        }
                    }

                }

            }
        }

        //On datagrid row click, opens the file
        private void resultDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as SpeakerRow;

                if (item != null)
                {
                    string fileType = item.tableName.ToString();
                    string fileName = item.ID.ToString();
                    string projectName = item.ProjectName.ToString();


                    //Check if file exists locally, if not open from db
                    if (File.Exists("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + item.Speaker + "\\" + fileName + "." + fileType))
                    {
                        openOrPlayLocalFile("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + item.Speaker + "\\" + fileName + "." + fileType);
                    }
                    else
                    {
                        using ((myConn = new DBConnection().getConn()))
                        using (MySqlCommand cmd = myConn.CreateCommand())
                        {
                            myConn.Open();
                            try
                            {
                                cmd.CommandText = "SELECT File FROM " + fileType + " where ID = '" + fileName + "'";
                                //cmd.Parameters.AddWithValue("@tName", tableName); //THIS DONT WORK. WHY? WHO KNOWS
                                //cmd.Parameters.AddWithValue("@fName", fileName);

                                openOrPlayFile(cmd, fileName, fileType, projectName, item);

                            }
                            catch (MySqlException ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }
                    }

                }

            }
        }

        private void openOrPlayLocalFile(string filePath)
        {

            Process.Start(filePath);
        }

        private void openOrPlayFile(MySqlCommand cmd, string fileName, string fileType, string projectName, Row row)
        {
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {

                int bufferSize; //mediumblob buffer size
                byte[] rawData;
                FileStream fs;
                string filePath = "";

                if (!reader.HasRows)
                    throw new Exception("There are no blobs to save");

                while (reader.Read())
                {
                    //Checks for the file and then puts it in the corect folder location
                    if (row is AnalysisRow)
                    {
                        Directory.CreateDirectory("..\\..\\..\\..\\testOutput\\ANALYSIS");
                        fs = new FileStream("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + fileName + "." + fileType, FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    else
                    {
                        Directory.CreateDirectory("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + ((SpeakerRow)row).Speaker);
                        fs = new FileStream("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + ((SpeakerRow)row).Speaker + "\\" + fileName + "." + fileType, FileMode.OpenOrCreate, FileAccess.Write);
                    }




                    int ndx = reader.GetOrdinal("File");
                    bufferSize = (int)reader.GetBytes(ndx, 0, null, 0, 0);  //get the length of data
                    rawData = new byte[bufferSize];

                    filePath = fs.Name;
                    BinaryWriter bw = new BinaryWriter(fs);

                    // Reset the starting byte for the new BLOB.
                    int startIndex = 0;
                    int bytesRead = 0;

                    while (startIndex < bufferSize)
                    {
                        bytesRead = (int)reader.GetBytes(ndx, startIndex,
                           rawData, startIndex, bufferSize - startIndex);
                        bw.Write(rawData);
                        bw.Flush();
                        startIndex += bytesRead;
                    }


                }


                //Fixed access denied error
                File.SetAttributes(filePath, FileAttributes.Normal);

                // Filter audio, images etc. to open appropriate program
                if (fileType.Equals("wav") || fileType.Equals("WAV"))
                {
                    mediaElement.Source = new Uri(filePath, UriKind.RelativeOrAbsolute);
                    mediaElement.LoadedBehavior = MediaState.Manual;
                    //mediaElement.UnloadedBehavior = MediaState.Stop;
                    mediaElement.Play();
                }
                else
                {
                    //Process.Start("notepad++.exe", path);
                    try
                    {
                        Process.Start(filePath);
                    }
                    catch
                    {
                        MessageBox.Show("Access denied, please run application with administrator privileges.");
                    }
                }
            }
        }


        //Testing, randomly macthes analysis to files
        private void randomlyMatchAnalysis()
        {
            using ((myConn = new DBConnection().getConn()))
            using (MySqlCommand cmd = myConn.CreateCommand())
            {
                myConn.Open();
                MySqlDataReader myReader;
                List<string> IDlist = new List<string>();
                List<string> AIDlist = new List<string>();
                cmd.CommandText = @"(SELECT ID FROM WAV) UNION (SELECT ID FROM hlb) UNION (SELECT ID FROM sf0) UNION
                                                        (SELECT ID FROM lab) UNION
                                                        (SELECT ID FROM sfb) UNION
                                                        (SELECT ID FROM wav) UNION
                                                        (SELECT ID FROM trg) ";

                using (myReader = cmd.ExecuteReader())
                {
                    while (myReader.Read())
                    {
                        IDlist.Add(myReader.GetString("ID"));
                    }
                    MessageBox.Show(IDlist.Count.ToString());

                    cmd.CommandText = "SELECT AID FROM analysis";
                    using (myReader = cmd.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            AIDlist.Add(myReader.GetString("AID"));
                        }
                    }
                    foreach (string ID in IDlist)
                    {


                        Random random = new Random();
                        int randomNumber = random.Next(0, 10);
                        for (int i = 0; i < randomNumber; i++)
                        {
                            cmd.CommandText = "INSERT IGNORE INTO files2analysis (ID, AID) VALUES (@ID, @AID)";
                            cmd.Parameters.AddWithValue("@ID", ID);
                            cmd.Parameters.AddWithValue("@AID", AIDlist[random.Next(0, AIDlist.Count - 1)]);
                            cmd.ExecuteNonQuery();

                        }

                    }

                }

            }

        }


        //Loads all the data in the target folder into the db
        private void loadAllButton_Click(object sender, RoutedEventArgs e)
        {
            using ((myConn = new DBConnection().getConn()))
            using (MySqlCommand cmd = myConn.CreateCommand())
            {
                myConn.Open();
                DirectoryInfo[] dirs = new DirectoryInfo(testDBRoot).GetDirectories();
                byte[] rawData;
                //Adds all files selected into folders to the db

                try
                {
                    foreach (DirectoryInfo dir in dirs)
                    {


                        FileInfo[] files = new DirectoryInfo(dir.FullName).GetFiles("*.*", SearchOption.AllDirectories);

                        //If analysis table exists, add them separately
                        if (dir.Name.Equals("ANALYSIS"))
                        {
                            //Create analysis table
                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS analysis (AID varchar(150) primary key, File mediumblob, Description varchar(500), FileType varchar(20))";
                            cmd.ExecuteNonQuery();

                            foreach (FileInfo file in files)
                            {
                                string fileName = Path.GetFileNameWithoutExtension(file.FullName);
                                rawData = File.ReadAllBytes(@file.FullName); //The raw file data as  a byte array

                                cmd.CommandText = "INSERT INTO analysis (AID, File, Description, FileType) VALUES (@AID, @fileAsBlob, @desc, @type)";
                                cmd.Parameters.AddWithValue("@AID", fileName);
                                cmd.Parameters.AddWithValue("@fileAsBlob", rawData);
                                cmd.Parameters.AddWithValue("@desc", "No description");
                                cmd.Parameters.AddWithValue("@type", file.Extension);
                                cmd.ExecuteNonQuery();

                            }

                        }
                        else
                        {


                            //Create projects table

                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS projects (PID varchar(150) primary key)";
                            cmd.ExecuteNonQuery();


                            cmd.CommandText = "INSERT IGNORE INTO projects (PID) VALUES (@PID)"; //ignore = Dont insert dups
                            MessageBox.Show(dir.ToString());
                            cmd.Parameters.AddWithValue("@PID", dir);
                            cmd.ExecuteNonQuery();

                            foreach (FileInfo file in files)
                            {

                                string fileName = Path.GetFileNameWithoutExtension(file.FullName);

                                string ext = Path.GetExtension(file.Name).Replace(".", "");
                                rawData = File.ReadAllBytes(@file.FullName); //The raw file data as  a byte array
                                string speaker = fileName.Substring(0, 4);

                                //Create tables if they dont already exist
                                cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + ext + "(ID varchar(150) primary key, File mediumblob, Speaker varchar(20), ProjectName varchar(100))";
                                cmd.ExecuteNonQuery();

                                //Add file paths to the above table
                                cmd.CommandText = "INSERT INTO " + ext + " (ID, File, Speaker, ProjectName) VALUES (@ID, @fileAsBlob, @speaker, @projectName)";
                                cmd.Parameters.AddWithValue("@ID", fileName);
                                cmd.Parameters.AddWithValue("@fileAsBlob", rawData);
                                cmd.Parameters.AddWithValue("@speaker", speaker);
                                cmd.Parameters.AddWithValue("@projectName", dir);
                                cmd.ExecuteNonQuery();


                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                MessageBox.Show("ALL DONE");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Close();
        }

        //On exit, removes the temp dir
        private void OnApplicationExit(object sender, EventArgs e)
        {
            //DeleteDirectory(@"..\\..\\..\\..\\testOutput", true);
        }

        //Method to delete the temp dir
        public static void DeleteDirectory(string path)
        {
            DeleteDirectory(path, false);
        }

        public static void DeleteDirectory(string path, bool recursive)
        {

            // Get all files of the folder
            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                // Get the attributes of the file
                var attr = File.GetAttributes(f);

                // Is this file marked as 'read-only'?
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Yes... Remove the 'read-only' attribute, then
                    File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                }

                // Delete the file
                File.Delete(f);
            }

            // When we get here, all the files of the folder were
            // already deleted, so we just delete the empty folder
            Directory.Delete(path);
        }


        //Search the datagrid and filter
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {

                    // Collection which will take your ObservableCollection (the datagrid row item)
                    var itemSourceList = new CollectionViewSource() { Source = this.rowS };

                    //now we add our Filter
                    itemSourceList.Filter += new FilterEventHandler(searchFilter);

                    // ICollectionView the View/UI part 
                    ICollectionView itemlist = itemSourceList.View;
                    buildDatagridGroups(itemlist);
                    //dataGridFiles.ItemsSource = itemlist;

                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }

        }

        //Regex filter
        private void searchFilter(object sender, FilterEventArgs e)
        {
            var obj = e.Item as SpeakerRow;
            if (obj != null)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(obj.ID, searchBox.Text))
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            //var listOfItems = dataGridFiles.SelectedItems;
            //var grid = dataGridFiles;
            //var copyGrid = dataGridFiles;
            //for(int i = 0; i <= copyGrid.SelectedItems.Count; i++)
            //{
            //    string tableName = (grid.SelectedItems[i] as SpeakerRow).tableName;
            //    string idName = (grid.SelectedItems[i] as SpeakerRow).ID;
            //    //System.Console.WriteLine(tableName);
            //    //System.Console.WriteLine(idName);
            //    //System.Console.WriteLine(i);

            //    //SpeakerRow dgRow = (from r in rowS where (r.ID == idName && r.tableName == tableName) select r).SingleOrDefault();

            //    copyGrid.Items.Remove(grid.SelectedItems[i] as Row);
            //    if (conn.openConn() == true)
            //    {
            //        try
            //        {
            //            //Create tables if they dont already exist
            //            MySqlCommand comm = conn.getCommand();
            //            comm.CommandText = "DELETE FROM " + tableName + " WHERE ID=@idName";
            //            comm.Parameters.AddWithValue("@idName", idName);
            //            comm.ExecuteNonQuery();
            //        }
            //        catch (MySqlException ex)
            //        {
            //            MessageBox.Show(ex.ToString());
            //        }
            //        conn.closeConn();
            //    }
            //}


            //ListCollectionView collection = new ListCollectionView(rowS);
            //buildDatagridGroups(collection);

        }

        private void ButtonTemplate_Click(object sender, RoutedEventArgs e)
        {
            List<List<string>> listResults = GenerateTempPrompt.Prompt("Enter template name", "Generate template file", inputType: GenerateTempPrompt.InputType.Text);
            string pathName = @"C:\Users\Rodel\Documents\p4p\TemplateStr\";
            string ext = "tpl";
            if(listResults == null)
            {
                return;
            }

            pathName += listResults.First().First() + "." + ext;
            try
            {

                // Create a new file 
                using (FileStream fs = File.Create(pathName))
                {
                    //Add some text to file
                    Byte[] heading = new UTF8Encoding(true).GetBytes("! this file was generated by Cirilla \n \n");
                    fs.Write(heading, 0, heading.Length);

                    int count = 0;
                    //The second list must be sfb tracks and sf0 third.
                    foreach (List<string> lStr in listResults)
                    {
                        string trackClassName = "";
                        // skip first result list which contain tpl name and project path
                        if (count == 0)
                        {
                            count += 1;
                            string pathFiles = testDBRoot + "\\" + lStr[1] + "\\*";
                            byte[] pathFString = new UTF8Encoding(true).GetBytes("path lab " + pathFiles +
                                "\n" + "path trg " + pathFiles + "\n" + "path hlb " + pathFiles + "\n" + "path wav " + pathFiles + "\n" + "path sfb " + pathFiles + "\n \n");
                            fs.Write(pathFString, 0, pathFString.Length);

                            byte[] sampleWavString = new UTF8Encoding(true).GetBytes("track samples wav\n");
                            fs.Write(sampleWavString, 0, sampleWavString.Length);
                            continue;
                        }
                        if (lStr.LastOrDefault().Equals("sf0Id"))
                        {
                            trackClassName = "sf0";
                        }
                        else if (lStr.LastOrDefault().Equals("sfbId"))
                        {
                            trackClassName = "sfb";
                        }
                        foreach (string str in lStr)
                        {
                            if (str.Equals(lStr.Last()))
                            {
                                continue;
                            }
                            // make sure to fix hardcoded sfb
                            byte[] trackString = new UTF8Encoding(true).GetBytes("track " + str + " " + trackClassName + "\n");
                            fs.Write(trackString, 0, trackString.Length);
                        }
                    }

                    byte[] primaryExt = new UTF8Encoding(true).GetBytes("\nset PrimaryExtension wav \n");
                    fs.Write(primaryExt, 0, primaryExt.Length);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void speakerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;



            groupValue = typeItem.Name.ToString();

            buildDatagridGroups(new ListCollectionView(rowS));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProjectRow pr = dataGridProjects.SelectedValue as ProjectRow;
            MenuItem mi = sender as MenuItem;



            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string parentDir = Directory.GetParent(folder).Parent.Parent.Parent.FullName;

            //Open the project folder in explorer
            switch (mi.Name)
            {
                case "menuShow":
                    Process.Start("explorer.exe", parentDir + @"\testOutput\" + pr.PID);
                    break;

                case "menuRemoveLocal":
                    break;

                case "menuRemoveDB":
                    break;

                case "menuDownload":
                    downloadProject(parentDir + @"\testOutput\", pr.PID);
                    break;
            }
        }


        private void backgroundWorker_DoWork(
            object sender,
            DoWorkEventArgs e)
        {
            string[] parameters = e.Argument as string[];
            string PID = parameters[1];
            string path = parameters[0];

            this.Dispatcher.Invoke((Action)(() =>
            {
                prog.Show();
            }));

            int bufferSize;//mediumblob buffer size
            byte[] rawData;
            FileStream fs;
            string filePath = "";

            using (MySqlConnection tempConn = new DBConnection().getConn())
            using (var cmd = tempConn.CreateCommand())
            {
                tempConn.Open();
                foreach (string table in tableNames)
                {
                    if (!dgl.ignoreTables.Contains(table)) //Only the file tables
                    {

                        cmd.Connection = tempConn;
                        cmd.CommandText = "SELECT * FROM " + table + " WHERE ProjectName='" + PID + "'";
                        //cmd.Parameters.AddWithValue("@table", table);
                        //cmd.Parameters.AddWithValue("@PID", PID);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                int ndx = reader.GetOrdinal("File");
                                bufferSize = (int)reader.GetBytes(ndx, 0, null, 0, 0);  //get the length of data
                                rawData = new byte[bufferSize];

                                Directory.CreateDirectory(path + PID + @"\" + reader.GetString("Speaker"));
                                fs = new FileStream(path + PID + @"\" + reader.GetString("Speaker") + @"\" + reader.GetString("ID") + "." + table, FileMode.OpenOrCreate, FileAccess.Write);
                                filePath = fs.Name;
                                BinaryWriter bw = new BinaryWriter(fs);

                                // Reset the starting byte for the new BLOB.
                                int startIndex = 0;
                                int bytesRead = 0;

                                while (startIndex < bufferSize)
                                {
                                    bytesRead = (int)reader.GetBytes(ndx, startIndex,
                                       rawData, startIndex, bufferSize - startIndex);
                                    bw.Write(rawData);
                                    bw.Flush();
                                    startIndex += bytesRead;
                                }

                                // Write the remaining buffer.
                                bw.Write(rawData, 0, (int)bytesRead);
                                bw.Flush();

                                // Close the output file.
                                bw.Close();
                                fs.Close();
                            }

                        }
                    }
                }
            }
        }


        private void backgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {

            prog.Close();
        }

        private void downloadProject(string path, string PID)
        {

            BackgroundWorker backgroundWorker;

            // Instantiate BackgroundWorker and attach handlers to its 
            // DowWork and RunWorkerCompleted events.
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);

            string[] parameters = new string[] { path, PID };

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


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Open file system to select file(s)
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;


            Nullable<bool> result = dlg.ShowDialog();  // Display OpenFileDialog by calling ShowDialog method 
            byte[] rawData;
            List<Tuple<string, byte[]>> dataList = new List<Tuple<string, byte[]>>();

            // Add all files selected into the the db. If multiple files added, project destination is the same.
            foreach (String file in dlg.FileNames)
            {
                // Get the selected file name and display in a TextBox 
                if (result.HasValue == true && result.Value == true)
                {
                    rawData = File.ReadAllBytes(file);
                    dataList.Add(Tuple.Create(file, rawData));
                }
            }

            AnalysisMsgPrompt a = new AnalysisMsgPrompt(dgl, rowP);



            if (a.ShowDialog() == true)
            {
                dgl.loadSpeakers(a.PID);
                rowS = dgl.getCollection("S");
                foreach (var elem in rowS.ToList())
                    ((dynamic)rowS).Add((SpeakerRow)elem);


                using ((myConn = new DBConnection().getConn()))
                using (MySqlCommand comm = myConn.CreateCommand())
                {
                    myConn.Open();

                    foreach (var dataItem in dataList)
                    {


                        try
                        {
                            //Add to analysis table
                            comm.CommandText = "create table if not exists analysis (AID varchar(150) primary key, File mediumblob, Description varchar(500))";
                            comm.ExecuteNonQuery();

                            comm.CommandText = "INSERT INTO analysis (AID, File, Description) VALUES(@AID, @FileAsBlob, @Desc)";
                            comm.Parameters.AddWithValue("@AID", dataItem.Item1);
                            comm.Parameters.AddWithValue("@FileAsBlob", dataItem.Item2);
                            if (a.Desc.Equals(""))
                            {
                                comm.Parameters.AddWithValue("@Desc", "No description");
                            }
                            else
                            {

                                comm.Parameters.AddWithValue("@Desc", a.Desc);
                            }
                            comm.ExecuteNonQuery();

                            //Add to the mapping table(to link with speaker)
                            List<Row> startsWithAge = rowS.Where(s => ((SpeakerRow)s).Speaker.StartsWith(a.Age)).ToList();

                            MessageBox.Show(a.Age);
                            foreach (var row in rowS)
                            {

                                //comm.CommandText = "create table if not exists files2analysis (AID varchar(150) primary key, ID varchar(150) primary key)";
                                //comm.ExecuteNonQuery();
                                if (((SpeakerRow)row).Speaker.StartsWith(a.Age))
                                {


                                    comm.CommandText = "INSERT IGNORE INTO files2analysis (ID, AID) VALUES (@ID2, @AID2)";
                                    comm.Parameters.Clear();
                                    comm.Parameters.AddWithValue("@ID2", ((SpeakerRow)row).ID);
                                    comm.Parameters.AddWithValue("@AID2", dataItem.Item1);
                                    comm.ExecuteNonQuery();
                                }

                            }
                        }
                        catch (Exception)
                        {
                            //conn.handleException(e);
                        }
                    }

                }
            }


        }



        private void ButtonConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            testDBRoot = dialog.SelectedPath;
        }

    }
}



