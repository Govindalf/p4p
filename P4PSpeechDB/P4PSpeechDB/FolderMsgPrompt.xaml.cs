using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;


namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for FolderMsgPrompt.xaml
    /// </summary>
    public partial class FolderMsgPrompt : Window
    {

        public enum InputType
        {
            Text,
            Password
        }

        private string folderNameCB;
        DBConnection conn;

        public FolderMsgPrompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            InitializeComponent();
            conn = new DBConnection();
            fillCombo();
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);
            txtCreateFolder.Text = question;
            Title = title;
            txtFolderName.Text = defaultValue;
            txtFolderName.Visibility = Visibility.Visible;
        }

        // fill in the values of each project name in the combobox
        private void fillCombo() 
        {
            string query = "SELECT * FROM p4pdatabase.projects";
            MySqlDataReader myReader;
            MySqlCommand cmd = new MySqlCommand(query, conn.getConn());
            try
            {
                if (conn.openConn() == true)
                {
                    myReader = cmd.ExecuteReader();
                    while (myReader.Read())
                    {
                        string projectName = myReader.GetString("PID");
                        cbChooseFolder.Items.Add(projectName);
                        folderNameCB = (string) cbChooseFolder.SelectedValue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            } 
            finally 
            {
                conn.closeConn();
            }
        }

        public static string Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            FolderMsgPrompt inst = new FolderMsgPrompt(question, title, defaultValue, inputType);
            inst.ShowDialog();

            if (inst.DialogResult == true)
            {
                // if the user choose to create a new folder
                if (inst.txtFolderName.Text != defaultValue && inst.cbChooseFolder.SelectedValue == null)
                {
                    return inst.txtFolderName.Text;
                }
                // if the user choose to use an existing folder
                else if (inst.cbChooseFolder.SelectedValue != null)
                {
                    return (string) inst.cbChooseFolder.SelectedValue;
                }
                // if the user didn't choose any, should throw exception
                else
                {
                    return defaultValue;
                }
            }
            //System.Console.WriteLine();
            return null;
        }

        void PromptDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtFolderName.Focus();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
