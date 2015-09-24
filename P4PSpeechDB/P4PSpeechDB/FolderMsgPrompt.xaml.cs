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

/** Authors: Govindu Samarasinghe, Rodel Rojos
 *  Date: 2015
 * 
 *  Project: The Big Data Speech Processing Platform
 *  Project proposed by the ECE department of The University of Auckland
 */
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
            txtFolderDesc.Text = defaultValue;
            txtFolderName.Visibility = Visibility.Visible;
        }

        // fill in the values of each project name in the combobox
        private void fillCombo()
        {
            using (DBConnection db = new DBConnection())
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Project");
                var table = db.getFromDB(cmd);
                foreach (DataRow dr in table.Rows)
                {

                    string projectName = dr["PID"].ToString();
                    cbChooseFolder.Items.Add(projectName);
                    folderNameCB = (string)cbChooseFolder.SelectedValue;
                }


            }

        }

        public static List<string> Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            FolderMsgPrompt inst = new FolderMsgPrompt(question, title, defaultValue, inputType);
            List<string> folderDetails = new List<string>();
            inst.ShowDialog();

            if (inst.DialogResult == true)
            {
                // if the user choose to create a new folder
                if (inst.txtFolderName.Text != defaultValue && inst.cbChooseFolder.SelectedValue == null)
                {
                    folderDetails.Add(inst.txtFolderName.Text);
                    if (inst.txtFolderDesc.Text != defaultValue)
                    {
                        folderDetails.Add(inst.txtFolderDesc.Text);
                    }
                    return folderDetails;
                }
                // if the user choose to use an existing folder
                else if (inst.cbChooseFolder.SelectedValue != null)
                {
                    folderDetails.Add((string)inst.cbChooseFolder.SelectedValue);
                    return folderDetails;
                }
                // if the user didn't choose any, should throw exception
                else
                {
                    folderDetails.Add(defaultValue);
                    return folderDetails;
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
