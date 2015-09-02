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
    /// Interaction logic for GenerateTempPrompt.xaml
    /// </summary>
    public partial class GenerateTempPrompt : Window
    {
        public enum InputType
        {
            Text,
            Password
        }

        private string folderNameCB;
        private string formIdentifier = "Form";
        private string pitchIdentifier = "Pitch";
        private string sf0Name;
        private string sfbName;
        private int countForm = 1;
        private int countPitch = 1;
        private int moveDownForm = 27;
        private int moveDownPitch = 27;

        DBConnection conn;

        public GenerateTempPrompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
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
            string queryProject = "SELECT * FROM SpeechDB.projects";
            string querySf0 = "SELECT * FROM SpeechDB.trackOptions WHERE trackClass='sf0'";
            string querySfb = "SELECT * FROM SpeechDB.trackOptions WHERE trackClass='sfb'";

            MySqlDataReader myReader;
            MySqlCommand cmdProject = new MySqlCommand(queryProject, conn.getConn());
            MySqlCommand cmdSf0 = new MySqlCommand(querySf0, conn.getConn());
            MySqlCommand cmdSfb = new MySqlCommand(querySfb, conn.getConn());

            try
            {
                // the project drop down box
                if (conn.openConn() == true)
                {
                    myReader = cmdProject.ExecuteReader();
                    while (myReader.Read())
                    {
                        string projectName = myReader.GetString("PID");
                        cbChooseFolder.Items.Add(projectName);
                        folderNameCB = (string) cbChooseFolder.SelectedValue;
                    }
                    myReader.Close();

                    myReader = cmdSf0.ExecuteReader();
                    while (myReader.Read())
                    {
                        sf0Name = myReader.GetString("ID");
                        pitchTrack1.Items.Add(sf0Name);
                    }
                    myReader.Close();

                    myReader = cmdSfb.ExecuteReader();
                    while (myReader.Read())
                    {
                        sfbName = myReader.GetString("ID");
                        formTrack1.Items.Add(sfbName);
                    }
                    myReader.Close();
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

        public static string[] Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            GenerateTempPrompt inst = new GenerateTempPrompt(question, title, defaultValue, inputType);
            inst.ShowDialog();
            string [] templateNames = new string[2];

            if (inst.DialogResult == true)
            {
                // if the user choose to create a new folder
                if (inst.txtFolderName.Text != defaultValue)
                {
                    // if the user choose to use an existing folder
                    templateNames[0] = inst.txtFolderName.Text;
                    if (inst.cbChooseFolder.SelectedValue != null)
                    {
                        templateNames[1] = (string)inst.cbChooseFolder.SelectedValue;
                        return templateNames;
                    }
                    else {
                        MessageBox.Show("Please choose a project folder for the template file.");
                        Prompt(question, title, defaultValue, inputType);
                    }
                    
                }
                
                // if the user didn't choose any, should throw exception
                else
                {
                    MessageBox.Show("Please enter a name for the template file.");
                    Prompt(question, title, defaultValue, inputType);
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

        private void CreateWPFComboBox(string trackId, int moveDown)
        {
            ComboBox cbox = new ComboBox();
            cbox.Width = 78;
            cbox.Margin = new Thickness(130,5 + moveDown,5.2,5);
            cbox.IsEditable = true;

            var converter = new BrushConverter();
            cbox.Background = (Brush) converter.ConvertFromString("#FFE4E0E0");
            
            //ComboBoxItem cboxitem3 = new ComboBoxItem();
            //cboxitem3.Content = "MSDN";
            //cbox.Items.Add(cboxitem3);
            if (trackId.Equals(formIdentifier)){
                cbox.Name = "formTrack" + countForm;
                cbForm.Children.Add(cbox);
            }
            else if (trackId.Equals(pitchIdentifier))
            {
                cbox.Name = "pitchTrack" + countForm;
                cbPitch.Children.Add(cbox);
            }

        }

        private void ButtonForformant_Click(object sender, RoutedEventArgs e)
        {
            countForm++;
            btForm.Margin = new Thickness(0, moveDownForm, 5.2, 5);
            CreateWPFComboBox(formIdentifier, moveDownForm);
            this.SizeToContent = SizeToContent.Height;
            System.Console.WriteLine(this.Width);
            moveDownForm += 32;

        }

        private void ButtonForPitch_Click(object sender, RoutedEventArgs e)
        {
            countPitch++;
            btPitch.Margin = new Thickness(0, moveDownPitch, 5.2, 5);

            CreateWPFComboBox(pitchIdentifier, moveDownPitch);
            this.SizeToContent = SizeToContent.Height;
            moveDownPitch += 32;
        }
    }
}
