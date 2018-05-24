/**-----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               VSDBS
 * Date Last Revision:  3/13/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the UI of the VSDBS. It backs the
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
using System.IO;
using System.Windows.Threading;
using System.Collections.ObjectModel;


namespace VSBDS
{

    public partial class MainWindow : Window
    {
        #region Variables
        // the index of the currently selected video
        private int selectedIndex = 0;
        
        // stores all image thumbnails
        private ObservableCollection<ImageSource> img_list = new
            ObservableCollection<ImageSource>();
        private List<String> videoLabels = new List<String>();

        #endregion

        /* ---------------------------------------------------------------------
         * LoadImages
         * ---------------------------------------------------------------------
         * Loads image thumbnails of each video's first frame. Loads the 
         * original video as the default video.
         */
        public void LoadImages()
        {
            //Image current_image;
            //String img_path = @"C:\users\phank\documents\visual studio 2017\Projects\WPFTutorial\WPFTutorial\Resources\Images\";
            String img_path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            img_path = img_path + "\\Resources\\Image\\";
            List<String> filenames = new List<String>(System.IO.Directory.EnumerateFiles(img_path, "*.jpg"));
            int imageCount = 0;
            BitmapImage image;
            foreach (String filename in filenames)
            {
                // put images in storage
                image = new BitmapImage(new Uri(filename));
                img_list.Add(image);
                imageCount++;
            }
            VideoListView.ItemsSource = img_list;
            loadVideoLabels();

            // set to play original video
            String videoPath = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
            videoPath = videoPath + "\\Resources\\20020924_juve_dk_02a.avi";
            mePlayer.Source = new Uri(videoPath);
            videoLabel.Content = "Choose Clip";

        }

        /* ---------------------------------------------------------------------
         * loadVideoLabels
         * ---------------------------------------------------------------------
         * Reads video labels from sceneindex.txt.
         * Precondition:
         *  -sceneindex.txt exists and in proper format
         */
        private void loadVideoLabels()
        {
            string line;
            StreamReader reader = new StreamReader("sceneindex.txt");
            while ((line = reader.ReadLine()) != null)
            {
                videoLabels.Add(line);
            }
        }

        /* ---------------------------------------------------------------------
         * VideoListView_SelectionChanged
         * ---------------------------------------------------------------------
         * When the user clicks on a thumbnail of a certain video, it changes
         * the media player's video to the proper video.
         */
        private void VideoListView_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            mePlayer.Stop();
            selectedIndex = VideoListView.SelectedIndex;
            String videoName = "";
            if (selectedIndex < 10) videoName += "000";
            else if (selectedIndex < 100) videoName += "00";
            else if (selectedIndex < 1000) videoName += "0";
            videoName += selectedIndex;
            String videoPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            videoPath = videoPath + "\\Resources\\Video\\" + videoName + ".avi";
            mePlayer.Source = new Uri(videoPath);
            videoLabel.Content = videoLabels[selectedIndex];
        }
    }
}
