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
using MySql.Data.MySqlClient;
using Path = System.IO.Path;

namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection myConn;
        public MainWindow()
        {
            try
            {
                string myConnection = "datasource = localhost; port = 3306; username = root; password = Cirilla_2015; database = p4pdatabase";
                myConn = new MySqlConnection(myConnection);
                //MySqlDataAdapter myDataAdapter = new MySqlDataAdapter();
                //myDataAdapter.SelectCommand = new MySqlCommand("select * database.edata;", myConn);
                //MySqlCommandBuilder cb = new MySqlCommandBuilder(myDataAdapter);
                myConn.Open();
                MessageBox.Show("Connected");
                myConn.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            InitializeComponent();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            loadDataGrid();
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
                
                //add if myConn is not null
                myConn.Open();

                string caseSwitch = ext;
                switch(caseSwitch){
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
                        break;

                }
                myConn.Close();
                
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
            } catch(Exception ex){
                MessageBox.Show(ex.Message);

            }
        }

        private void loadDataGrid() { 
            try 
            {
                myConn.Open();
                MySqlCommand cmd = new MySqlCommand("Select ID, hlbPath from hlb", myConn);
                MySqlDataAdapter adp = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adp.Fill(ds, "LoadDataBinding");
                dataGridFiles.DataContext = ds;
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
