using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private InputType _inputType = InputType.Text;

        public FolderMsgPrompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PromptDialog_Loaded);
            txtCreateFolder.Text = question;
            Title = title;
            txtFolderName.Text = defaultValue;
            txtFolderName.Visibility = Visibility.Visible;
        }

        public static string Prompt(string question, string title, string defaultValue = "", InputType inputType = InputType.Text)
        {
            FolderMsgPrompt inst = new FolderMsgPrompt(question, title, defaultValue, inputType);
            inst.ShowDialog();
            if (inst.DialogResult == true)
                return inst.txtFolderName.Text;
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
