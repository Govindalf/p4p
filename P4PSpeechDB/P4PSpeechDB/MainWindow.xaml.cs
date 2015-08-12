using System;
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
                    string query = "show tables from p4pdatabase";
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
                        comm.CommandText = "create table if not exists " + ext + "(ID varchar(150) primary key, " + pathNameVar + " varchar(500))";
                        comm.ExecuteNonQuery();

                        //Add file paths to the above table
                        comm = conn.getCommand();
                        comm.CommandText = "INSERT INTO " + ext + "(ID," + pathNameVar + ") VALUES(@ID, @pathNameVar)";
                        comm.Parameters.AddWithValue("@ID", fileName);
                        comm.Parameters.AddWithValue("@pathNameVar", path.FullName);
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

            // Add all files selected into the the db. If multiple files added, project destination is the same.
            foreach (String file in dlg.FileNames) 
            {
                // Get the selected file name and display in a TextBox 
                if (result.HasValue == true && result.Value == true)
                {
                    // Open document 
                    string filename = file;
                    string ext = Path.GetExtension(file);
                    string pathNameVar = "filePath";

                    //Stores file in appropriate place in file system
                    //moveFile(filename, databaseRoot  /* + WHATEVER THE NEW LOCATION IS ASK CATH */);

                    // Connect to the mysql db if possible
                    if (conn.openConn() == true)
                    {
                        try
                        {
                            MySqlCommand comm = conn.getCommand();
                            comm.CommandText = "create table if not exists " + ext.Substring(1) + "(ID varchar(150) primary key, " + pathNameVar + " varchar(100), ProjectName varchar(100))";
                            comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);

                        }
                        executeInsert(filename, ext, dlg, folderName);

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

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg, string folderName)
        {
            string pathNameVar = "filePath";

            try
            {

                MySqlCommand comm = conn.getCommand();
                comm.CommandText = "INSERT INTO " + ext.Substring(1) + "(ID," + pathNameVar + ", ProjectName) VALUES(@ID, @pathNameVar, @ProjectName)";
                comm.Parameters.AddWithValue("@ID", Path.GetFileNameWithoutExtension(dlg.SafeFileName));
                comm.Parameters.AddWithValue("@pathNameVar", filename);
                comm.Parameters.AddWithValue("@ProjectName", folderName);
                comm.ExecuteNonQuery();
                
                if(folderName != null){
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
                ObservableCollection<DBFile> dbFile = new ObservableCollection<DBFile>();
                MySqlDataReader myReader;

                try
                {

                    //Get number of tables in database, for all tables, do the following
                    DataSet ds = new DataSet();
                    foreach (string name in Tablenames)
                    {
                        //Exclude the projects table
                        if(name.Equals("projects")){
                            continue;
                        }
                        //System.Console.WriteLine(name);
                        MySqlCommand cmd = new MySqlCommand("Select ID, filePath, ProjectName  from " + name, conn.getConn());
                        //MySqlDataAdapter adp = new MySqlDataAdapter(cmd);

                        myReader = cmd.ExecuteReader();
                        while (myReader.Read())
                        {
                            string projectName = "default";
                            if(myReader.GetValue(2).ToString() != ""){
                                projectName = myReader.GetValue(2).ToString();
                            }

                            dbFile.Add(new DBFile { ID = myReader.GetString("ID"), filePath = myReader.GetString("filePath"), ProjectName = projectName });
                        }
                        myReader.Close();

                        //adp.Fill(ds, "LoadDataBinding");
                        //dataGridFiles.DataContext = ds;

                    }
                    ListCollectionView collection = new ListCollectionView(dbFile);
                    collection.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
                    dataGridFiles.ItemsSource = collection;

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            conn.closeConn();
        }

        private void resultDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var item = dgr.DataContext as DBFile;

                if (item != null)
                {
                    var fileP = item.filePath;
                    string path = fileP.ToString();
                    openOrPlayFile(path);
                   
                }

            }
        }

        private void openOrPlayFile(string path)
        {
            // Filter audio, images etc. to open appropriate program
            Process.Start("notepad++.exe", path);
        }


    }
}
