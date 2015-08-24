using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.ComponentModel;
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
using System.ComponentModel;

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
        private List<String> Tablenames = new List<String>();
        private ObservableCollection<DatagridRow> row = new ObservableCollection<DatagridRow>(); //DAtagrid row item

        public MainWindow()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnApplicationExit);

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
                            Tablenames.Add(reader.GetString(0));
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
            loadDataGrid();
        }

        // depreciated
        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            loadDataGrid();

            if (conn.openConn() == true)
            {
                FileInfo[] paths = new DirectoryInfo(databaseRoot).GetFiles("*.*", SearchOption.AllDirectories);

                //Adds all files selected into folders to the db
                foreach (FileInfo path in paths)
                {
                    string ext = Path.GetExtension(path.Name).Replace(".", "");
                    string fileName = Path.GetFileNameWithoutExtension(path.FullName);
                    string pathNameVar = "filePath";

                    try
                    {

                        //Create tables if they dont already exist
                        MySqlCommand comm = conn.getCommand();
                        comm.CommandText = "create table if not exists " + ext + "(ID varchar(150) primary key, " + pathNameVar + " varchar(500), ProjectName varchar(100))";
                        comm.ExecuteNonQuery();

                        //Add file paths to the above table
                        comm = conn.getCommand();
                        comm.CommandText = "INSERT INTO " + ext + "(ID," + pathNameVar + ") VALUES(@ID, @pathNameVar, @projectName)";
                        comm.Parameters.AddWithValue("@ID", fileName);
                        comm.Parameters.AddWithValue("@pathNameVar", path.FullName);
                        comm.Parameters.AddWithValue("@projectName", "DefaultProject");
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                    }

                }
                conn.closeConn();
            }

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
            loadDataGrid();

        }

        private string getFolderName()
        {
            return FolderMsgPrompt.Prompt("Create new folder", "Folder options", inputType: FolderMsgPrompt.InputType.Text);

        }

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg, string folderName, byte[] rawData)
        {
            filename = Path.GetFileNameWithoutExtension(dlg.SafeFileName);
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

        private void loadDataGrid()
        {
            if (conn.openConn() == true)
            {
               
                MySqlDataReader myReader;

                try
                {

                    //Get number of tables in database, for all tables, do the following
                    DataSet ds = new DataSet();
                    foreach (string name in Tablenames)
                    {
                        //Exclude the projects table
                        if (name.Equals("projects"))
                        {
                            continue;
                        }
                        //System.Console.WriteLine(name);
                        //MySqlCommand cmd = new MySqlCommand("Select ID, filePath, ProjectName  from " + name, conn.getConn());

                        MySqlCommand cmd = new MySqlCommand("Select ID, ProjectName, Speaker  from " + name, conn.getConn());
                        //MySqlDataAdapter adp = new MySqlDataAdapter(cmd);

                        myReader = cmd.ExecuteReader();
                        while (myReader.Read())
                        {
                            string projectName = "default";
                            if (myReader.GetValue(1).ToString() != "")
                            {
                                projectName = myReader.GetValue(1).ToString();
                            }

                            //dbFile.Add(new DBFile { ID = myReader.GetString("ID"), filePath = myReader.GetString("filePath"), ProjectName = projectName });
                            row.Add(new DatagridRow { ID = myReader.GetString("ID"), ProjectName = projectName, Speaker = myReader.GetString("Speaker"), tableName = name });
                           
                        }
                        myReader.Close();
                        //adp.Fill(ds, "LoadDataBinding");
                        //dataGridFiles.DataContext = ds;

                    }

                    //Pass in the collection made of the datagrid rows
                    ListCollectionView collection = new ListCollectionView(row);
                    buildDatagridGroups(collection);


                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            conn.closeConn();
        }

        //Sets up the grouping for the datagrid
        private void buildDatagridGroups(ICollectionView collection) 
        {
            PropertyGroupDescription propertyDes = new PropertyGroupDescription("ProjectName");

            collection.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
            collection.GroupDescriptions.Add(new PropertyGroupDescription("Speaker"));

            dataGridFiles.ItemsSource = collection;
            dataGridFiles.Items.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
        }



        //On datagrid row click, opens the file
        private void resultDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as DatagridRow;
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (conn.openConn() == true)
            {
                FileInfo[] files = new DirectoryInfo(testDBRoot).GetFiles("*.*", SearchOption.AllDirectories);
                byte[] rawData;
                //Adds all files selected into folders to the db

                try
                {
                    foreach (FileInfo file in files)
                    {

                        string fileName = Path.GetFileNameWithoutExtension(file.FullName);

                        string ext = Path.GetExtension(file.Name).Replace(".", "");
                        rawData = File.ReadAllBytes(@file.FullName); //The raw file data as  a byte array
                        string speaker = fileName.Substring(0, 4);

                        //Create tables if they dont already exist
                        MySqlCommand comm = conn.getCommand();
                        comm.CommandText = "create table if not exists " + ext + "(ID varchar(150) primary key, File mediumblob, Speaker varchar(20), ProjectName varchar(100))";
                        comm.ExecuteNonQuery();

                        //Add file paths to the above table
                        comm = conn.getCommand();
                        comm.CommandText = "INSERT INTO " + ext + " (ID, File, Speaker, ProjectName) VALUES (@ID, @fileAsBlob, @speaker, @projectName)";
                        comm.Parameters.AddWithValue("@ID", fileName);
                        comm.Parameters.AddWithValue("@fileAsBlob", rawData);
                        comm.Parameters.AddWithValue("@speaker", speaker);
                        comm.Parameters.AddWithValue("@projectName", "DefaultProject");
                        comm.ExecuteNonQuery();


                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                conn.closeConn();
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
                    var itemSourceList = new CollectionViewSource() { Source = this.row };

                    //now we add our Filter
                    itemSourceList.Filter += new FilterEventHandler(searchFilter);

                    // ICollectionView the View/UI part 
                    ICollectionView itemlist = itemSourceList.View;
                    
                    buildDatagridGroups(itemlist);
                    //dataGridFiles.ItemsSource = itemlist;
                    gridcontrol
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
            var obj = e.Item as DatagridRow;
            if (obj != null)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(obj.ID, searchBox.Text))
                    e.Accepted = true;
                else
                    e.Accepted = false;
            }
        }





    }
}
