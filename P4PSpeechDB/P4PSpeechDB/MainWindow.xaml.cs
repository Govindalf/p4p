﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
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

namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string databaseRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles"; //Where the P4Ptestfiles folder is
        //private string testDBRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\TestDB";
        private string testDBRoot = "C:\\Users\\Rodel\\Documents\\p4p\\P4Ptestfiles";
        private DBConnection conn;
        private List<String> Tablenames = new List<String>();

        public MainWindow()
        {

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
                            comm.CommandText = "create table if not exists " + ext.Substring(1) + "(ID varchar(150) primary key, File mediumblob, ProjectName varchar(100))";
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
            string pathNameVar = "File";

            try
            {

                MySqlCommand comm = conn.getCommand();
                comm.CommandText = "INSERT INTO " + ext.Substring(1) + "(ID," + pathNameVar + ", ProjectName) VALUES(@ID, @FileAsBlob, @ProjectName)";
                comm.Parameters.AddWithValue("@ID", Path.GetFileNameWithoutExtension(dlg.SafeFileName));
                comm.Parameters.AddWithValue("@FileAsBlob", rawData);
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
                ObservableCollection<DatagridRow> row = new ObservableCollection<DatagridRow>();
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

                        MySqlCommand cmd = new MySqlCommand("Select ID, File, ProjectName  from " + name, conn.getConn());
                        //MySqlDataAdapter adp = new MySqlDataAdapter(cmd);

                        myReader = cmd.ExecuteReader();
                        while (myReader.Read())
                        {
                            string projectName = "default";
                            if (myReader.GetValue(2).ToString() != "")
                            {
                                projectName = myReader.GetValue(2).ToString();
                            }

                            //dbFile.Add(new DBFile { ID = myReader.GetString("ID"), filePath = myReader.GetString("filePath"), ProjectName = projectName });

                            row.Add(new DatagridRow { ID = myReader.GetString("ID"), ProjectName = projectName, tableName = name });
                            ListCollectionView collection = new ListCollectionView(row);

                            collection.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
                            dataGridFiles.ItemsSource = collection;
                        }
                        myReader.Close();
                        //adp.Fill(ds, "LoadDataBinding");
                        //dataGridFiles.DataContext = ds;

                    }

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            conn.closeConn();
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
                                fs = new FileStream("..\\..\\..\\..\\testOutput\\" + fileName + ".wav", FileMode.OpenOrCreate, FileAccess.Write);
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

                            openOrPlayFile(filePath);
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

        private void openOrPlayFile(string path)
        {
            // Filter audio, images etc. to open appropriate program
            //Process.Start("notepad++.exe", path);
            MessageBox.Show(path);
            mediaElement.Source = new Uri(path, UriKind.RelativeOrAbsolute);
            mediaElement.LoadedBehavior = MediaState.Manual;
            //mediaElement.UnloadedBehavior = MediaState.Stop;
            mediaElement.Play();
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


                        //Create tables if they dont already exist
                        MySqlCommand comm = conn.getCommand();
                        comm.CommandText = "create table if not exists test" + ext + "(ID varchar(150) primary key, File mediumblob, ProjectName varchar(100))";
                        comm.ExecuteNonQuery();

                        //Add file paths to the above table
                        comm = conn.getCommand();
                        comm.CommandText = "INSERT INTO test" + ext + " (ID, File, ProjectName) VALUES (@ID, @fileAsBlob, @projectName)";
                        comm.Parameters.AddWithValue("@ID", fileName);
                        comm.Parameters.AddWithValue("@fileAsBlob", rawData);
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


    }
}
