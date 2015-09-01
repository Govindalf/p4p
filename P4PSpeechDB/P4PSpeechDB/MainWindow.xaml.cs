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

namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string databaseRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles"; //Where the P4Ptestfiles folder is
        private string testDBRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\TestDB";
        private DBConnection conn;

        private List<String> tableNames = new List<String>();
        private ObservableCollection<Row> rowS; //DAtagrid row item
        private ObservableCollection<Row> rowA; //DAtagrid row item
        DataGridLoader dgl;

        public Boolean IsExpanded { get; set; }


        public MainWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnApplicationExit);
            IsExpanded = false;
            this.DataContext = this;

            conn = new DBConnection();

            try
            {
                conn.openConn();

                // store all of the tables in the mysql database into a list
                using (conn.getConn())
                {
                    string query = "show tables from SpeechDB";
                    MySqlCommand command = new MySqlCommand(query, conn.getConn());
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tableNames.Add(reader.GetString(0));
                        }
                    }
                }
                conn.closeConn();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            InitializeComponent();
            //randomlyMatchAnalysis();
            //Loads all datagrid with relevant data
            dgl = new DataGridLoader(conn, tableNames);
            dgl.setUpDataGrids();
            rowS = dgl.getCollection("S");
            buildDatagridGroups(new ListCollectionView(rowS));

            dataGridProjects.ItemsSource = dgl.getCollection("P");

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
            string folderName = getFolderName(); // only prompt for folder once always
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
                    if (conn.openConn() == true)
                    {
                        try
                        {
                            MySqlCommand comm = conn.getCommand();
                            comm.CommandText = "create table if not exists " + ext.Substring(1) + "(ID varchar(150) primary key, File mediumblob, Speaker varchar(20), ProjectName varchar(100))";
                            comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);

                        }
                        executeInsert(filename, ext, dlg, folderName, rawData);

                        conn.closeConn();
                    }
                }
            }
            dgl.setUpDataGrids();

        }

        private string getFolderName()
        {
            return FolderMsgPrompt.Prompt("Create new folder", "Folder options", inputType: FolderMsgPrompt.InputType.Text);

        }

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg, string folderName, byte[] rawData)
        {

            filename = Path.GetFileName(filename);
            string speaker = Path.GetFileNameWithoutExtension(dlg.SafeFileName).Substring(0, 4);

            try
            {

                MySqlCommand comm = conn.getCommand();
                comm.CommandText = "INSERT INTO " + ext.Substring(1) + "(ID, File, Speaker, ProjectName) VALUES(@ID, @FileAsBlob, @Speaker, @ProjectName)";
                comm.Parameters.AddWithValue("@ID", filename);
                comm.Parameters.AddWithValue("@FileAsBlob", rawData);
                comm.Parameters.AddWithValue("@Speaker", speaker);
                comm.Parameters.AddWithValue("@ProjectName", folderName);
                comm.ExecuteNonQuery();

                if (folderName != null)
                {
                    comm.CommandText = "INSERT IGNORE INTO projects(PID) VALUES(@PID)";
                    comm.Parameters.AddWithValue("@PID", folderName);
                    comm.ExecuteNonQuery();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        //Sets up the grouping for the datagrid
        private void buildDatagridGroups(ICollectionView collection)
        {
            PropertyGroupDescription propertyDes = new PropertyGroupDescription("ProjectName");
            collection.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));

            dataGridFiles.ItemsSource = collection;
            dataGridFiles.Items.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
        }


        //When a row in the projects grid is selected
        private void dataGridProjects_GotCellFocus(object sender, RoutedEventArgs e)
        {
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

        }

        //On datagrid row click, opens the file
        private void resultDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as SpeakerRow;
                MySqlDataReader reader;

                int bufferSize = 16777215; //mediumblob buffer size
                byte[] rawData = new byte[bufferSize];
                FileStream fs;

                if (item != null)
                {
                    string tableName = item.tableName.ToString();
                    string fileName = item.ID.ToString();
                    string filePath = "";
                    if (conn.openConn() == true)
                    {

                        try
                        {
                            MySqlCommand cmd = new MySqlCommand();
                            cmd.Connection = conn.getConn();
                            cmd.CommandText = "SELECT File FROM " + tableName + " where ID = '" + fileName + "'";
                            //cmd.Parameters.AddWithValue("@tName", tableName); //THIS DONT WORK. WHY? WHO KNOWS
                            //cmd.Parameters.AddWithValue("@fName", fileName);
                            reader = cmd.ExecuteReader();
                            if (!reader.HasRows)
                                throw new Exception("There are no blobs to save");

                            while (reader.Read())
                            {


                                Directory.CreateDirectory("..\\..\\..\\..\\testOutput");
                                fs = new FileStream("..\\..\\..\\..\\testOutput\\" + fileName + "." + tableName, FileMode.OpenOrCreate, FileAccess.Write);
                                filePath = fs.Name;
                                BinaryWriter bw = new BinaryWriter(fs);

                                // Reset the starting byte for the new BLOB.
                                long startIndex = 0;

                                // Read the bytes into outbyte[] and retain the number of bytes returned.
                                long retval = reader.GetBytes(0, startIndex, rawData, 0, bufferSize);

                                // Continue reading and writing while there are bytes beyond the size of the buffer.
                                while (retval == bufferSize)
                                {
                                    bw.Write(rawData);
                                    bw.Flush();

                                    // Reposition the start index to the end of the last buffer and fill the buffer.
                                    startIndex += bufferSize;
                                    retval = reader.GetBytes(1, startIndex, rawData, 0, bufferSize);
                                }

                                // Write the remaining buffer.
                                bw.Write(rawData, 0, (int)retval);
                                bw.Flush();

                                // Close the output file.
                                bw.Close();
                                fs.Close();

                            }

                            openOrPlayFile(filePath, tableName);
                        }
                        catch (MySqlException ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        conn.closeConn();
                    }

                }

            }
        }

        private void openOrPlayFile(string path, string fileType)
        {

            // Filter audio, images etc. to open appropriate program
            if (fileType.Equals("wav") || fileType.Equals("WAV"))
            {
                mediaElement.Source = new Uri(path, UriKind.RelativeOrAbsolute);
                mediaElement.LoadedBehavior = MediaState.Manual;
                //mediaElement.UnloadedBehavior = MediaState.Stop;
                mediaElement.Play();
            }
            else
            {
                Process.Start("notepad++.exe", path);
            }

        }


        //Testing, randomly macthes analysis to files
        private void randomlyMatchAnalysis()
        {
            if (conn.openConn() == true)
            {
                MySqlCommand comm;
                MySqlDataReader myReader;
                List<string> IDlist = new List<string>();
                List<string> AIDlist = new List<string>();
                comm = conn.getCommand();
                MySqlCommand cmd = new MySqlCommand(@"(SELECT ID FROM WAV) UNION (SELECT ID FROM hlb) UNION (SELECT ID FROM sf0) UNION
                                                        (SELECT ID FROM lab) UNION
                                                        (SELECT ID FROM sfb) UNION
                                                        (SELECT ID FROM wav) UNION
                                                        (SELECT ID FROM trg) ", conn.getConn());

                myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    IDlist.Add(myReader.GetString("ID"));
                }
                MessageBox.Show(IDlist.Count.ToString());
                myReader.Close();

                comm = conn.getCommand();
                cmd = new MySqlCommand("SELECT AID FROM analysis", conn.getConn());
                myReader = cmd.ExecuteReader();
                while (myReader.Read())
                {
                    AIDlist.Add(myReader.GetString("AID"));
                }
                myReader.Close();

                foreach (string ID in IDlist)
                {


                    Random random = new Random();
                    int randomNumber = random.Next(0, 10);
                    for (int i = 0; i < randomNumber; i++)
                    {
                        comm = conn.getCommand();
                        comm.CommandText = "INSERT IGNORE INTO files2analysis (ID, AID) VALUES (@ID, @AID)";
                        comm.Parameters.AddWithValue("@ID", ID);
                        comm.Parameters.AddWithValue("@AID", AIDlist[random.Next(0, AIDlist.Count-1)]);
                        comm.ExecuteNonQuery();

                    }



                }

                conn.closeConn();

            }

        }


        //Loads all the data in the target folder into the db
        private void loadAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (conn.openConn() == true)
            {

                DirectoryInfo[] dirs = new DirectoryInfo(testDBRoot).GetDirectories();
                byte[] rawData;
                //Adds all files selected into folders to the db

                try
                {
                    foreach (DirectoryInfo dir in dirs)
                    {


                        FileInfo[] files = new DirectoryInfo(dir.FullName).GetFiles("*.*", SearchOption.AllDirectories);
                        MySqlCommand comm;

                        //If analysis table exists, add them separately
                        if (dir.Name.Equals("ANALYSIS"))
                        {
                            //Create analysis table
                            comm = conn.getCommand();
                            comm.CommandText = "CREATE TABLE IF NOT EXISTS analysis (AID varchar(150) primary key, File mediumblob, Description varchar(500))";
                            comm.ExecuteNonQuery();

                            foreach (FileInfo file in files)
                            {
                                string fileName = Path.GetFileNameWithoutExtension(file.FullName);
                                rawData = File.ReadAllBytes(@file.FullName); //The raw file data as  a byte array

                                comm = conn.getCommand();
                                comm.CommandText = "INSERT INTO analysis (AID, File, Description) VALUES (@AID, @fileAsBlob, @desc)";
                                comm.Parameters.AddWithValue("@AID", fileName);
                                comm.Parameters.AddWithValue("@fileAsBlob", rawData);
                                comm.Parameters.AddWithValue("@desc", "No description");
                                comm.ExecuteNonQuery();

                            }

                        }
                        else
                        {


                            //Create projects table

                            comm = conn.getCommand();
                            comm.CommandText = "CREATE TABLE IF NOT EXISTS projects (PID varchar(150) primary key)";
                            comm.ExecuteNonQuery();


                            comm = conn.getCommand();
                            comm.CommandText = "INSERT IGNORE INTO projects (PID) VALUES (@PID)"; //ignore = Dont insert dups
                            MessageBox.Show(dir.ToString());
                            comm.Parameters.AddWithValue("@PID", dir);
                            comm.ExecuteNonQuery();

                            foreach (FileInfo file in files)
                            {

                                string fileName = Path.GetFileNameWithoutExtension(file.FullName);

                                string ext = Path.GetExtension(file.Name).Replace(".", "");
                                rawData = File.ReadAllBytes(@file.FullName); //The raw file data as  a byte array
                                string speaker = fileName.Substring(0, 4);

                                //Create tables if they dont already exist
                                comm = conn.getCommand();
                                comm.CommandText = "CREATE TABLE IF NOT EXISTS " + ext + "(ID varchar(150) primary key, File mediumblob, Speaker varchar(20), ProjectName varchar(100))";
                                comm.ExecuteNonQuery();

                                //Add file paths to the above table
                                comm = conn.getCommand();
                                comm.CommandText = "INSERT INTO " + ext + " (ID, File, Speaker, ProjectName) VALUES (@ID, @fileAsBlob, @speaker, @projectName)";
                                comm.Parameters.AddWithValue("@ID", fileName);
                                comm.Parameters.AddWithValue("@fileAsBlob", rawData);
                                comm.Parameters.AddWithValue("@speaker", speaker);
                                comm.Parameters.AddWithValue("@projectName", dir);
                                comm.ExecuteNonQuery();


                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                conn.closeConn();
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
            //foreach (var i in dataGridFiles.SelectedItems)
            //{
            //    string tableName = (i as SpeakerRow).tableName;
            //    string idName = (i as SpeakerRow).ID;
            //    System.Console.WriteLine(tableName);
            //    System.Console.WriteLine(idName);
            //    System.Console.WriteLine(i);
            //    SpeakerRow dgRow = (from r in row where (r.ID == idName && r.tableName == tableName) select r).SingleOrDefault();
            //    //newRow.Remove(dgRow);
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

            //collection = new ListCollectionView(row);
            //buildDatagridGroups(collection);

        }

        private void ButtonTemplate_Click(object sender, RoutedEventArgs e)
        {

            // Enter a sf0 and sfb file
            // Find all lines with columns
            // Get string and split by space
            // get 2nd substring
            // write in file, track + samples + wav
            // write in file, track + 2nd substring + sfb/sf0

            string [] tempName = GenerateTempPrompt.Prompt("Enter template name", "Generate template file", inputType: GenerateTempPrompt.InputType.Text);
            string pathName = @"..\\..\\..\\..\\TemplateStr\";
            string ext = "tpl";

            pathName += tempName[0] + "." + ext;
            System.Console.WriteLine(pathName);

            try
            {
                string sfbFile = "";
                List<String> wordToTrack = new List<string>();

                // Delete if file exists. 
                if (File.Exists(pathName))
                {
                    File.Delete(pathName);
                }

                if (conn.openConn() == true)
                {
                    MySqlCommand comm = conn.getCommand();
                    comm.CommandText = "SELECT File FROM sfb WHERE ProjectName=@projName";
                    comm.Parameters.AddWithValue("@projName", tempName[1]);
                    using (MySqlDataReader reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sfbFile = reader.GetString(0);
                        }
                    }
                    //comm.ExecuteNonQuery();
                }
                conn.closeConn();
                if (sfbFile != null)
                {
                    int startText = sfbFile.IndexOf("Column");
                    int endText = sfbFile.IndexOf("---");
                    string filteredText = sfbFile.Substring(startText, endText);
                    string[] splitByColumn = filteredText.Split(new string[] { "Column" }, StringSplitOptions.None);
                    for (int i = 1; i <splitByColumn.Length ; i++)
                    {
                        string[] splitBySpace = splitByColumn[i].Split(new char[0]);
                        wordToTrack.Add(splitBySpace[1]);
                    }

                }

                // Create a new file 
                using (FileStream fs = File.Create(pathName))
                {
                    // Add some text to file
                    Byte[] heading = new UTF8Encoding(true).GetBytes("! this file was generated by Cirilla \n \n");
                    fs.Write(heading, 0, heading.Length);

                    byte[] sampleWavString = new UTF8Encoding(true).GetBytes("track samples wav\n");
                    fs.Write(sampleWavString, 0, sampleWavString.Length);

                    foreach (string str in wordToTrack){
                        // make sure to fix hardcoded sfb
                        byte[] trackString = new UTF8Encoding(true).GetBytes("track " + str + " sfb\n");
                        fs.Write(trackString, 0, trackString.Length);
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

        

    }
}
