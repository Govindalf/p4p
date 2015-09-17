using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
            //conn.createDB();
            //Loads all datagrid with relevant data
            dgl = new DataGridLoader(conn, tableNames);
            dgl.setUpDataGrids();
            rowS = dgl.getCollection("S");
            rowP = dgl.getCollection("P");

            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.speakerCombo.Text = groupValue;

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

                    executeInsert(filename, ext, dlg, folderDetails, rawData);


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
            var path = Path.GetExtension(filename);
            filename = Path.GetFileName(filename);
            string speaker = "";

            if (dlg != null)
            {
                speaker = Path.GetFileNameWithoutExtension(dlg.SafeFileName).Substring(0, 4);
            }
            else
            {
                speaker = "Template";
            }

            using (DBConnection db = new DBConnection())
            {

                if (folderDetails != null)
                {

                    MySqlCommand comm = new MySqlCommand();
                    comm.CommandText = "INSERT INTO File (PID, Name, FileType, Speaker) VALUES(@PID, @Name, @Type, @Speaker)";
                    comm.Parameters.AddWithValue("@Name", filename);
                    comm.Parameters.AddWithValue("@Type", path);
                    comm.Parameters.AddWithValue("@Speaker", speaker);
                    comm.Parameters.AddWithValue("@PID", folderDetails.First());
                    db.insertIntoDB(comm);

                    comm = new MySqlCommand();

                    comm.CommandText = "INSERT INTO FileData (FID, FileData) VALUES (LAST_INSERT_ID(), @FileData)";
                    comm.Parameters.AddWithValue("@FileData", rawData);
                    db.insertIntoDB(comm);

                    comm = new MySqlCommand();
                    comm.CommandText = "INSERT IGNORE INTO Project (PID, DateCreated, Description) VALUES(@PID, @dateCreated, @description)";
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
                    db.insertIntoDB(comm);
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


        /*When a row in the projects grid is selected, load the relevent speech data into the
         * speech datagrid, and do so efficiently.*/
        private void dataGridProjects_GotCellFocus(object sender, RoutedEventArgs e)
        {
            this.emptyGrid.Visibility = System.Windows.Visibility.Hidden;
            string projectName = "";

            if (e.OriginalSource.GetType() == typeof(DataGridCell) && sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as ProjectRow;

                if (item != null)
                {
                    projectName = item.PID.ToString();
                    dgl.loadSpeakers(projectName);
                    rowS = dgl.getCollection("S");

                    buildDatagridGroups(new ListCollectionView(rowS));
                }

            }

            using ((myConn = new DBConnection().getConn()))
            using (MySqlCommand cmd = myConn.CreateCommand())
            {
                try
                {
                    myConn.Open();
                    if (!projectName.Equals(""))
                    {
                        cmd.CommandText = "SELECT Description FROM Project WHERE PID = '" + projectName + "'";
                    }
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            descTextBlock.Text = reader.GetString(0);
                        }
                    }

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
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
                    string ID = item.Name;
                    dgl.loadAnalysis(ID);
                    rowA = dgl.getCollection("A");
                    dataGridAnalysis.ItemsSource = new ListCollectionView(rowA);
                    //buildDatagridGroups(new ListCollectionView(rowA));
                }
            }
        }

        /*When analysis datagrid item clicked, opens the selected item. */
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
                        using (DBConnection db = new DBConnection())
                        {

                            var cmd = new MySqlCommand();
                            cmd.CommandText = "SELECT FileData FROM Analysis where AID = @fileName";
                            cmd.Parameters.AddWithValue("@fileName", fileName);

                            //call the download and save method
                            openOrPlayFile(cmd, fileName, fileType, "ANALYSIS", item);


                        }
                    }
                }
            }
        }

        /*When speech file datagrid item clicked, opens the file. */
        private void resultDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as SpeakerRow;

                if (item != null)
                {
                    string fileType = item.FileType.ToString();
                    string fileName = item.Name.ToString();
                    string projectName = item.PID.ToString();


                    //Check if file exists locally, if not open from db
                    if (File.Exists("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + item.Speaker + "\\" + fileName + "." + fileType))
                    {
                        openOrPlayLocalFile("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + item.Speaker + "\\" + fileName + "." + fileType);
                    }
                    else
                    {

                        var cmd = new MySqlCommand();
                        cmd.CommandText = @"SELECT f.FileData FROM FileData f INNER JOIN File fi ON fi.FID = f.FID WHERE fi.Name= '" + fileName + "' AND fi.FileType = @Type";
                        //cmd.Parameters.AddWithValue("@tName", tableName); //THIS DONT WORK. WHY? WHO KNOWS
                        cmd.Parameters.AddWithValue("@Type", fileType);

                        openOrPlayFile(cmd, fileName, fileType, projectName, item);

                    }

                }

            }
        }

        private void openOrPlayLocalFile(string filePath)
        {

            Process.Start(filePath);
        }

        /*Downlaods and opens the file selected, or plays it if its a .wav(audio) */
        private void openOrPlayFile(MySqlCommand cmd, string fileName, string fileType, string projectName, Row row)
        {
            using (DBConnection db = new DBConnection())
            {

                byte[] rawData;
                FileStream fs;
                string filePath = "";

                //Checks for the file type (speaker or analysis) and then puts it in the correct folder location
                if (row is AnalysisRow)
                {
                    Directory.CreateDirectory("..\\..\\..\\..\\testOutput\\ANALYSIS");
                    fs = new FileStream("..\\..\\..\\..\\testOutput\\" + projectName + "\\" + fileName + fileType, FileMode.OpenOrCreate, FileAccess.Write);
                }
                else
                {
                    Directory.CreateDirectory(@"..\..\..\..\testOutput\" + projectName + "\\" + ((SpeakerRow)row).Speaker);
                    fs = new FileStream(@"..\..\..\..\testOutput\" + projectName + "\\" + ((SpeakerRow)row).Speaker + "\\" + fileName + fileType, FileMode.OpenOrCreate, FileAccess.Write);
                }

                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {

                    rawData = (byte[])dr["FileData"]; // convert successfully to byte[]


                    filePath = fs.Name;

                    //Fixed access denied error
                    File.SetAttributes(filePath, FileAttributes.Normal);

                    // Writes a block of bytes to this stream using data from
                    // a byte array.
                    fs.Write(rawData, 0, rawData.Length);

                    // close file stream
                    fs.Close();

                }


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

            using (DBConnection db = new DBConnection())
            {
                var cmd = new MySqlCommand();
                cmd.CommandText = @"SELECT FID FROM File";

                var tableF = db.getFromDB(cmd);
                cmd = new MySqlCommand();

                cmd.CommandText = @"SELECT AID FROM Analysis";
                var tableA = db.getFromDB(cmd);

                MessageBox.Show(tableA.Rows.Count.ToString());
                foreach (DataRow dr in tableF.Rows)
                {
                    foreach (DataRow drA in tableA.Rows)
                    {

                        Random random = new Random();
                        int randomNumber = random.Next(0, 10);
                        System.Diagnostics.Debug.WriteLine(randomNumber);
                        if (randomNumber < 2)
                        {
                            cmd = new MySqlCommand();
                            cmd.CommandText = "INSERT IGNORE INTO File2Analysis (File_FID, Analysis_AID) VALUES (@FID2, @AID)";
                            cmd.Parameters.AddWithValue("@FID2", dr["FID"].ToString());
                            cmd.Parameters.AddWithValue("@AID", drA["AID"].ToString());
                            db.insertIntoDB(cmd);

                        }
                    }
                }

            }

            MessageBox.Show("DONE");

        }




        //Loads all the data in the target folder into the db
        private void loadAllButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = promptForFolder();
            if (!result.ToString().Equals("Cancel"))
            {
                using (DBConnection db = new DBConnection())
                {
                    //testDBRoot = "C:\\Users\\Rodel\\Documents\\p4p\\P4Ptestfiles";
                    DirectoryInfo dirN = null;
                    DirectoryInfo[] dirs = new DirectoryInfo(testDBRoot).GetDirectories();
                    string projDescription = "None";

                    // If there are no directories inside the chosen path.
                    if (dirs == null || dirs.Length == 0)
                    {
                        dirN = new DirectoryInfo(testDBRoot);
                    }

                    List<String> projectDetails = getFolderName();
                    if (projectDetails != null && dirN == null)
                    {
                        if (projectDetails.Count > 1)
                        {
                            projDescription = projectDetails.Last();
                        }
                        foreach (DirectoryInfo dir in dirs)
                        {
                            addIndividualFile(dir, projectDetails, projDescription, db);
                        }
                    }
                    else if (dirN != null)
                    {
                        addIndividualFile(dirN, projectDetails, projDescription, db);
                    }

                }
                MessageBox.Show("ALL DONE");
            }
        }

        private void addIndividualFile(DirectoryInfo dir, List<String> projectDetails, string projDescription, DBConnection db) 
        {
            byte[] rawData;

            FileInfo[] files = new DirectoryInfo(dir.FullName).GetFiles("*.*", SearchOption.AllDirectories);

            //Create projects table
            var cmd = new MySqlCommand();
            cmd.CommandText = "INSERT IGNORE INTO Project (PID, DateCreated, Description) VALUES (@PID, @date, @desc)"; //ignore = Dont insert dups

            cmd.Parameters.AddWithValue("@PID", projectDetails.First());
            cmd.Parameters.AddWithValue("@date", DateTime.Today);
            cmd.Parameters.AddWithValue("@desc", projDescription);
            db.insertIntoDB(cmd);

            foreach (FileInfo file in files)
            {

                string fileName = Path.GetFileNameWithoutExtension(file.FullName);

                string ext = Path.GetExtension(file.Name).Replace(".", "");
                rawData = File.ReadAllBytes(@file.FullName); //The raw file data as  a byte array
                string speaker = fileName.Substring(0, 4);

                //Add file paths to the above table
                cmd = new MySqlCommand();
                cmd.CommandText = "INSERT INTO File (PID, Name, FileType, Speaker) VALUES (@PID, @Name, @FileType, @Speaker)";
                cmd.Parameters.AddWithValue("@PID", projectDetails.First());
                cmd.Parameters.AddWithValue("@Name", fileName);
                cmd.Parameters.AddWithValue("@FileType", file.Extension);
                cmd.Parameters.AddWithValue("@Speaker", speaker);
                db.insertIntoDB(cmd);

                //Add file data
                cmd = new MySqlCommand();
                cmd.CommandText = "INSERT INTO FileData (FID, FileData) VALUES (LAST_INSERT_ID(), @data)";
                cmd.Parameters.AddWithValue("@data", rawData);
                db.insertIntoDB(cmd);

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
                if (Regex.IsMatch(obj.Name, searchBox.Text, RegexOptions.IgnoreCase))
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var listOfItems = dataGridFiles.SelectedItems;
            var grid = dataGridFiles;
            var copyGrid = dataGridFiles;
            for (int i = 0; i <= copyGrid.SelectedItems.Count; i++)
            {
                string idName = (grid.SelectedItems[i] as SpeakerRow).ID;
                //System.Console.WriteLine(tableName);
                //System.Console.WriteLine(idName);
                //System.Console.WriteLine(i);

                SpeakerRow dgRow = (SpeakerRow) (from r in rowS where ((r as SpeakerRow).ID == idName) select r).SingleOrDefault();
                System.Console.WriteLine(dgRow.ID);
                //copyGrid.Items.Remove(grid.SelectedItems[i] as Row);
                //if (conn.openConn() == true)
                //{
                //    try
                //    {
                //        //Create tables if they dont already exist
                //        MySqlCommand comm = conn.getCommand();
                //        comm.CommandText = "DELETE FROM File WHERE ID=@idName";
                //        comm.Parameters.AddWithValue("@idName", idName);
                //        comm.ExecuteNonQuery();
                //    }
                //    catch (MySqlException ex)
                //    {
                //        MessageBox.Show(ex.ToString());
                //    }
                //    conn.closeConn();
                //}
            }


            //ListCollectionView collection = new ListCollectionView(rowS);
            //buildDatagridGroups(collection);

        }

        private void ButtonTemplate_Click(object sender, RoutedEventArgs e)
        {
            List<List<string>> listResults = GenerateTempPrompt.Prompt("Enter template name", "Generate template file", inputType: GenerateTempPrompt.InputType.Text);
            string pathName = @"C:\Users\Rodel\Documents\p4p\TemplateStr\";
            string ext = "tpl";
            List<string> projN = new List<string>();
            if (listResults == null)
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
                    Byte[] heading = new UTF8Encoding(true).GetBytes("! this file was generated by Moa \n \n");
                    fs.Write(heading, 0, heading.Length);

                    int count = 0;
                    //The second list must be sfb tracks and sf0 third.
                    foreach (List<string> lStr in listResults)
                    {
                        string trackClassName = "";
                        // skip first result list which contain tpl name and project path
                        if (count == 0)
                        {
                            projN.Add(lStr[1]);
                            count += 1;
                            string pathFiles = testDBRoot + "\\" + projN + "\\*";
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
                            count += 1;
                        }
                        else if (lStr.LastOrDefault().Equals("sfbId"))
                        {
                            trackClassName = "sfb";
                            count += 1;
                        }

                        else
                        {
                            trackClassName = lStr.Last();
                            count += 1;
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
                byte[] rawData = File.ReadAllBytes(pathName);
                executeInsert(pathName, ext, null, projN, rawData);


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


        /* Downloads a selected project on a background thread. */
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


            byte[] rawData;
            FileStream fs;
            string filePath = "";

            /* Get file data for every file in project and save it in the correct folder structure. */
            using (DBConnection db = new DBConnection())
            {
                var cmd = new MySqlCommand();
                cmd.CommandText = "SELECT f.Name, f.Speaker, f.FileType, fd.FileData FROM FileData fd INNER JOIN File f ON f.FID = fd.FID WHERE f.PID = @PID";
                cmd.Parameters.AddWithValue("@PID", PID);

                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {

                    rawData = (byte[])dr["FileData"];

                    Directory.CreateDirectory(path + PID + @"\" + dr["Speaker"].ToString());
                    fs = new FileStream(path + PID + @"\" + dr["Speaker"].ToString() + @"\" + dr["Name"].ToString() + dr["FileType"].ToString(), FileMode.OpenOrCreate, FileAccess.Write);

                    filePath = fs.Name;

                    //Fixed access denied error
                    File.SetAttributes(filePath, FileAttributes.Normal);

                    // Writes a block of bytes to this stream using data from
                    // a byte array.
                    fs.Write(rawData, 0, rawData.Length);

                    // close file stream
                    fs.Close();
                }

            }
        }


        /* Called when the background thread completes. */
        private void backgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            prog.Close();
        }

        /* Downloads projects on a different thread to UI thread, so user can do other tasks while downloading. */
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
            promptForFolder();
        }

        private System.Windows.Forms.DialogResult promptForFolder()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            testDBRoot = dialog.SelectedPath;
            return result;
        }

        private void ButtonWebMaus_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://clarin.phonetik.uni-muenchen.de/BASWebServices/#/services/WebMAUSGeneral");
        }

        private void ButtonAddFolder_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}



