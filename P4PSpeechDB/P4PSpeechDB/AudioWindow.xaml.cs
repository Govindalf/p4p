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

/** Authors: Govindu Samarasinghe, Rodel Rojos
 *  Date: 2015
 * 
 *  Project: The Big Data Speech Processing Platform
 *  Project proposed by the ECE department of The University of Auckland
 */

namespace P4PSpeechDB
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AudioWindow : Window
    {
        public AudioWindow(string filePath)
        {
            InitializeComponent();

            mediaElement.Source = new Uri(filePath, UriKind.RelativeOrAbsolute);
            mediaElement.LoadedBehavior = MediaState.Manual;
            //mediaElement.UnloadedBehavior = MediaState.Stop;
            mediaElement.Play();
        }


        private void Button_Click_Play(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
        }

        private void Button_Click_Pause(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Close();
        }
    }
}
