/**-----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               VSDBS
 * Date Last Revision:  3/13/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the buttons of the VSDBS. It backs the
 * MainWindow.xaml.
 * -----------------------------------------------------------------------------
 */

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Video.FFMPEG;
using System.Drawing;

namespace VSBDS
{

    public partial class MainWindow : Window
    {
        /* ---------------------------------------------------------------------
         * timerTick
         * ---------------------------------------------------------------------
         * Keeps track of the video's time.
         */
        void timerTick(object sender, EventArgs e)
        {
            if (mePlayer.Source != null)
            {
                if (mePlayer.NaturalDuration.HasTimeSpan)
                    lblStatus.Content = String.Format("{0} / {1}", mePlayer.Position.ToString(@"mm\:ss"), mePlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            }
            else
                lblStatus.Content = "No file selected...";
        }

        /* ---------------------------------------------------------------------
         * btnPlay_Click
         * ---------------------------------------------------------------------
         * Plays the current video.
         */
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Play();
        }

        /* ---------------------------------------------------------------------
         * btnPause_Click
         * Pauses the current video.
         */
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        /* ---------------------------------------------------------------------
         * btnStop_Click
         * ---------------------------------------------------------------------
         * Stops the current video.
         */
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
        }

        /* ---------------------------------------------------------------------
         * btnOrig_Click
         * ---------------------------------------------------------------------
         * Stops the currently playing video and sets the media player's video
         * to the original video.
         */
        private void btnOrig_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
            String videoPath = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
            videoPath = videoPath + "\\Resources\\20020924_juve_dk_02a.avi";
            mePlayer.Source = new Uri(videoPath);
            videoLabel.Content = "Choose Clip";
        }
    }
}
