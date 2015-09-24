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
    /// Interaction logic for GenerateTempPrompt.xaml
    /// 
    /// Responsible for the functionality of the generate template dialog window. This class enables users to create an
    /// Emu template file and choose the different options available in the path, track and primary extension levels.
    /// The generated template file will be stored in the default directory path specified in the users setting file 
    /// (More information in MoaCore.cs) unless changed by the user through the path settings button. 
    /// 
    /// Future work:
    /// - Inserting the template file into the database as a file and attaching it to its corresponding project.
    /// - Implementing the rest of the Emu template file features. Can be achieved with search indexing within files.
    /// - More user input options.
    /// </summary>
    public partial class GenerateTempPrompt : Window
    {
        public enum InputType
        {
            Text,
            Password
        }

        private string folderNameCB;
        private static string formIdentifier = "sfb";
        private static string pitchIdentifier = "sf0";
        private static string otherIdentifier = "other";
        private string sf0Name;
        private string sfbName;
        //private int countForm = 1;
        //private int countPitch = 1;
        private int moveDownForm = 27;
        private int moveDownPitch = 27;
        private int moveDownOther = 27;

        private static List<String> pitchOptions = new List<String>();
        private static List<String> formOptions = new List<String>();
        private static List<String> otherOptions = new List<String>();

        private static List<ComboBox> pitchCB = new List<ComboBox>();
        private static List<ComboBox> formCB = new List<ComboBox>();
        private static List<TextBox> otherCB = new List<TextBox>();

        private static List<List<string>> returnList = new List<List<string>>();
        private static List<string> nonTrackList = new List<string>();
        private static List<string> sf0ReturnList = new List<string>();
        private static List<string> sfbReturnList = new List<string>();
        private static List<string> otherTrackList = new List<string>();


        private DBConnection conn;

        public GenerateTempPrompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            InitializeComponent();
            conn = new DBConnection();
            fillCombo();
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);
            txtCreateFolder.Text = question;
            Title = title;
            txtFolderName.Text = defaultValue;
            txtTrackOther.Text = defaultValue;
            txtFolderName.Visibility = Visibility.Visible;
        }

        // fill in the values of each project name in the combobox
        private void fillCombo() 
        {
            string queryProject = "SELECT * FROM SpeechDB.Project";
            string querySf0 = "SELECT * FROM SpeechDB.Track WHERE class='sf0'";
            string querySfb = "SELECT * FROM SpeechDB.Track WHERE class='sfb'";

            MySqlDataReader myReader;
            MySqlCommand cmdProject = new MySqlCommand(queryProject, conn.getConn());
            MySqlCommand cmdSf0 = new MySqlCommand(querySf0, conn.getConn());
            MySqlCommand cmdSfb = new MySqlCommand(querySfb, conn.getConn());

            try
            {
                // the project drop down box
                if (conn.openConn() == true)
                {
                    //Get all the projects from db
                    myReader = cmdProject.ExecuteReader();
                    while (myReader.Read())
                    {
                        string projectName = myReader.GetString("PID");
                        cbChooseFolder.Items.Add(projectName);
                        folderNameCB = (string) cbChooseFolder.SelectedValue;
                    }
                    myReader.Close();

                    //Get all the track options for sf0 from db
                    myReader = cmdSf0.ExecuteReader();
                    while (myReader.Read())
                    {
                        sf0Name = myReader.GetString("TID");
                        pitchOptions.Add(sf0Name);
                        pitchTrack1.Items.Add(sf0Name);
                        
                    }
                    myReader.Close();

                    //Get all the track options for sfb from db
                    myReader = cmdSfb.ExecuteReader();
                    while (myReader.Read())
                    {
                        sfbName = myReader.GetString("TID");
                        formOptions.Add(sfbName);
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
        
        /** Called by the Moa Core after user presses the generate template button */
        public static List<List<string>> Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            returnList = new List<List<string>>();
            nonTrackList = new List<string>();
            sf0ReturnList = new List<string>();
            sfbReturnList = new List<string>();

            pitchOptions = new List<String>();
            formOptions = new List<String>();
            pitchCB = new List<ComboBox>();
            formCB = new List<ComboBox>();
            GenerateTempPrompt inst = new GenerateTempPrompt(question, title, defaultValue, inputType);
            
            inst.ShowDialog();
            string [] templateNames = new string[2];

            if (inst.DialogResult == true)
            {
                // user must enter a template name
                if (inst.txtFolderName.Text != defaultValue)
                {
                    // user must choose an existing project
                    nonTrackList.Add(inst.txtFolderName.Text);

                    if (inst.cbChooseFolder.SelectedValue != null)
                    {
                        // non track list gathers all the user input not associated to the track level, this statement gets existing project
                        nonTrackList.Add((string)inst.cbChooseFolder.SelectedValue);
                        returnList.Add(nonTrackList);

                        // if the user selected a formant track value to add to the template file
                        if (inst.formTrack1.SelectedValue != null)
                        {
                            sfbReturnList.Add((string)inst.formTrack1.SelectedValue);                            
                        }
                        else if (!inst.formTrack1.Text.Equals(defaultValue))
                        {
                            sfbReturnList.Add(inst.formTrack1.Text);                            

                        }

                        // when the user has added more comboboxes for sfb, check to see if more track values were selected
                        if (formCB.Count >= 1)
                        {
                            // Future works. Need to get only distinct.
                            foreach (ComboBox cb in formCB)
                            {
                                if (cb.SelectedValue != null && !sfbReturnList.Contains(cb.SelectedValue))
                                {
                                    sfbReturnList.Add((string)cb.SelectedValue);
                                }
                                else if (!cb.Text.Equals(defaultValue) && !sfbReturnList.Contains(cb.Text))
                                {
                                    sfbReturnList.Add(cb.Text);
                                }
                            }
                        }

                        if (sfbReturnList.Count > 0)
                        {
                            inst.insertToTable(formIdentifier);
                            sfbReturnList.Add("sfbId");
                            returnList.Add(sfbReturnList);
                        }

                        // For the Pitch track class. if the user selected a pitch track value to add to the template file
                        if (inst.pitchTrack1.SelectedValue != null)
                        {
                            sf0ReturnList.Add((string)inst.pitchTrack1.SelectedValue);
                        }

                        else if (!inst.pitchTrack1.Text.Equals(defaultValue))
                        {
                            sf0ReturnList.Add(inst.pitchTrack1.Text);

                        }
                        // when the user has added more comboboxes for sf0, check to see if more track values were selected
                        if (pitchCB.Count >= 1)
                        {
                            foreach (ComboBox cb in pitchCB)
                            {
                                if (cb.SelectedValue != null && !sf0ReturnList.Contains(cb.SelectedValue))
                                    sf0ReturnList.Add((string)cb.SelectedValue);
                                else if (!cb.Text.Equals(defaultValue) && !sf0ReturnList.Contains(cb.Text))
                                    sf0ReturnList.Add(cb.Text);
                            }
                        }
                        if (sf0ReturnList.Count > 0)
                        {
                            inst.insertToTable(pitchIdentifier);
                            sf0ReturnList.Add("sf0Id");
                            returnList.Add(sf0ReturnList);
                        }
                        return returnList;
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

        /** Opens the database connection and attempts to add new track options to the Track table.*/
        private void insertToTable(string trackID) 
        {
            List<string> distinctList = new List<string>();
            if(trackID.Equals(formIdentifier))
            {
                distinctList = sfbReturnList.Except(formOptions).ToList();
                
            }
            else if(trackID.Equals(pitchIdentifier))
            {
                distinctList = sf0ReturnList.Except(pitchOptions).ToList();
            }

            try
            {
                if (conn.openConn() == true)
                {
                    MySqlCommand comm = conn.getCommand();
                    if (distinctList.Count != 0)
                    {
                        foreach (string str in distinctList)
                        {
                            comm.CommandText = "INSERT IGNORE INTO Track (TID, class) VALUES (@ID, @trackClass)";
                            comm.Parameters.AddWithValue("@ID", str);
                            comm.Parameters.AddWithValue("@trackClass", trackID);
                            comm.ExecuteNonQuery();
                        }
                    }
                }
                conn.closeConn();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        /** Create a new combobox/text input dynamically and place it in the appropriate location*/
        private void CreateWPFComboBox(string trackId, int moveDown)
        {
            ComboBox cbox = new ComboBox();
            cbox.Width = 78;
            cbox.Margin = new Thickness(130,5 + moveDown,5.2,5);
            cbox.IsEditable = true;

            var converter = new BrushConverter();
            cbox.Background = (Brush) converter.ConvertFromString("#FFE4E0E0");
            
            if (trackId.Equals(formIdentifier))
            {
                //cbox.Name = "formTrack" + countForm;
                foreach (string str in formOptions)
                {
                    cbox.Items.Add(str);
                }
                cbForm.Children.Add(cbox);   
                formCB.Add(cbox);
            }
            else if (trackId.Equals(pitchIdentifier))
            {
                //cbox.Name = "pitchTrack" + countPitch;
                foreach (string str in pitchOptions)
                {
                    cbox.Items.Add(str);
                }
                cbPitch.Children.Add(cbox);
                pitchCB.Add(cbox);
            }
            else if (trackId.Equals(otherIdentifier))
            {
                TextBox tbox = new TextBox();
                tbox.Width = 78;
                tbox.Height = 25;
                tbox.Margin = new Thickness(60, 5 + moveDown, 5.2, 5);
                cbOther.Children.Add(tbox);
                //cbox.Name = "otherTrack" + countOther;
                otherCB.Add(tbox);
            }

        }
        // Add more input boxes to add track options
        private void ButtonForformant_Click(object sender, RoutedEventArgs e)
        {

            btForm.Margin = new Thickness(0, moveDownForm, 5.2, 5);
            CreateWPFComboBox(formIdentifier, moveDownForm);
            this.SizeToContent = SizeToContent.Height;
            moveDownForm += 32;

        }

        // Add more input boxes to add track options
        private void ButtonForPitch_Click(object sender, RoutedEventArgs e)
        {

            btPitch.Margin = new Thickness(0, moveDownPitch, 5.2, 5);

            CreateWPFComboBox(pitchIdentifier, moveDownPitch);
            this.SizeToContent = SizeToContent.Height;
            moveDownPitch += 32;
        }

        // Add more input boxes to add track options
        private void ButtonForOther_Click(object sender, RoutedEventArgs e)
        {
            btOther.Margin = new Thickness(0, moveDownOther, 5.2, 5);
            CreateWPFComboBox(otherIdentifier, moveDownOther);
            this.SizeToContent = SizeToContent.Height;
            moveDownOther += 32;
        }
    }

}
