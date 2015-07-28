﻿using System;
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
        MySqlConnection myConn;
        List<String> Tablenames = new List<String>();
        public MainWindow()
        {
            try
            {
                string myConnection = "datasource = localhost; port = 3306; username = root; password = Cirilla_2015; database = p4pdatabase";
                myConn = new MySqlConnection(myConnection);

                // Get number of tables in database
                myConn.Open();
                using (myConn)
                {
                    string query = "show tables from p4pdatabase";
                    MySqlCommand command = new MySqlCommand(query, myConn);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tablenames.Add(reader.GetString(0));
                        }
                    }
                }
                System.Console.Write(Tablenames.Count);
                myConn.Close();

                //MySqlDataAdapter myDataAdapter = new MySqlDataAdapter();
                //myDataAdapter.SelectCommand = new MySqlCommand("select * database.edata;", myConn);
                //MySqlCommandBuilder cb = new MySqlCommandBuilder(myDataAdapter);
                //myConn.Open();
                //MessageBox.Show("Connected");
                //myConn.Close();

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

            myConn.Open();
            FileInfo[] paths = new DirectoryInfo("C:\\Users\\Govindu\\Dropbox\\P4P\\p4p\\P4Ptestfiles").GetFiles("*.*", SearchOption.AllDirectories);

            //Adds all files in folders to the db
            foreach (FileInfo path in paths)
            {
                string ext = Path.GetExtension(path.Name).Replace(".", "");
                string fileName = Path.GetFileNameWithoutExtension(path.FullName);
                string pathNameVar = ext + "Path";

                try
                {

                    //Create tables if they dont already exist
                    MySqlCommand comm = myConn.CreateCommand();
                    comm.CommandText = "create table if not exists " + ext + "(ID varchar(150) primary key, " + pathNameVar + " varchar(500))";
                    comm.ExecuteNonQuery();

                    //Add file paths to the above table
                    comm = myConn.CreateCommand();
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
            myConn.Close();

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
                string pathNameVar = ext.Substring(1) + "Path";

                //add if myConn is not null
                myConn.Open();


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
                            MySqlCommand comm = myConn.CreateCommand();
                            comm.CommandText = "create table if not exists " + ext.Substring(1) + "(ID varchar(150) primary key, " + pathNameVar + " varchar(500))";
                            comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);

                        }
                        Console.WriteLine(ext);
                        executeInsert(filename, ext, dlg);
                        break;


                }
                myConn.Close();
                loadDataGrid();

            }

        }

        private void executeInsert(String filename, String ext, Microsoft.Win32.OpenFileDialog dlg)
        {
            string pathNameVar = ext.Substring(1) + "Path";

            try
            {
                MySqlCommand comm = myConn.CreateCommand();
                comm.CommandText = "INSERT INTO " + ext.Substring(1) + "(ID," + pathNameVar + ") VALUES(@ID, @pathNameVar)";
                string[] splitBySlash = filename.Split('/');
                System.Console.Write(dlg.SafeFileName);
                comm.Parameters.AddWithValue("@ID", Path.GetFileNameWithoutExtension(dlg.SafeFileName));
                //comm.Parameters.AddWithValue("@ID", splitBySlash.Last());
                comm.Parameters.AddWithValue("@pathNameVar", filename);
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        private void loadDataGrid()
        {
            try
            {
                myConn.Open();
                //Get number of tables in database, for all tables, do the following
                DataSet ds = new DataSet();
                foreach (string name in Tablenames)
                {
                    //System.Console.WriteLine(name);
                    MySqlCommand cmd = new MySqlCommand("Select ID, " + name + "Path from " + name, myConn);
                    MySqlDataAdapter adp = new MySqlDataAdapter(cmd);

                    adp.Fill(ds, "LoadDataBinding");

                    dataGridFiles.DataContext = ds;

                }

            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                myConn.Close();
            }
        }
    }
}
