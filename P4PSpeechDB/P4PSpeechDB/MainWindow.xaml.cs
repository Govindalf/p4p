using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
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
        string databaseRoot = "C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles"; //Where the P4Ptestfiles folder is
        DBConnection conn;
        List<String> Tablenames = new List<String>();
        public MainWindow()
        {

            conn = new DBConnection();
            try
            {
                //string myConnection = "datasource = localhost; port = 3306; username = root; password = Cirilla_2015; database = p4pdatabase";
                //myConn = new MySqlConnection(myConnection);
                conn.openConn();
                // Get number of tables in database
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
                System.Console.Write(Tablenames.Count);
                conn.closeConn();



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            InitializeComponent();
            loadDataGrid();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            loadDataGrid();

            if (conn.openConn() == true)
            {
                FileInfo[] paths = new DirectoryInfo(databaseRoot).GetFiles("*.*", SearchOption.AllDirectories);

                //Adds all files in folders to the db
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
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".png";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result.HasValue == true && result.Value == true)
            {
                // Open document 
                string filename = dlg.FileName;
                string ext = Path.GetExtension(dlg.FileName);
                string pathNameVar = "filePath";

                //Stores file in appropriate place in file system
                //moveFile(filename, databaseRoot  /* + WHATEVER THE NEW LOCATION IS ASK CATH */);

                //add if myConn is not null
                if (conn.openConn() == true)
                {

                    string caseSwitch = ext;
                    switch (caseSwitch)
                    {
                        case ".hlb":
                        Console.WriteLine(ext);
                        executeInsert(filename, ext, dlg);
                        break;
                        case ".lab":
                            Console.WriteLine(ext);
                            executeInsert(filename, ext, dlg);
                            break;
                        case ".sf0":
                            Console.WriteLine(ext);
                            executeInsert(filename, ext, dlg);
                            break;
                        case ".sfb":
                            Console.WriteLine(ext);
                            executeInsert(filename, ext, dlg);
                            break;
                        case ".tpl":
                            Console.WriteLine(ext);
                            executeInsert(filename, ext, dlg);
                            break;
                        case ".trg":
                            Console.WriteLine(ext);
                            executeInsert(filename, ext, dlg);
                            break;
                        case ".wav":
                            Console.WriteLine(ext);
                            executeInsert(filename, ext, dlg);
                            break;
                        default:
                            //Create new MySql table
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
                            executeInsert(filename, ext, dlg);
                            break;


                    }
                    conn.closeConn();
                }
                loadDataGrid();
            }

        }

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg)
        {
            string pathNameVar = "filePath";

            try
            {

                MySqlCommand comm = conn.getCommand();
                string folderName = FolderMsgPrompt.Prompt("Create new folder", "Folder options", inputType: FolderMsgPrompt.InputType.Text);

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

    }
}
