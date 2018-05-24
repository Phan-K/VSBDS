/**-----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               VSDBS
 * Date Last Revision:  3/13/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the main window of the VSDBS. It backs the
 * MainWindow.xaml.
 * 
 * This program cuts up a video into scenes using the Twin Comparison Approach,
 * which measures the differences in intensity between frames and cuts them up
 * based on the difference thresholds. 
 * 
 * The user can interact with a UI that allows them to view the original video
 * and the cut up scenes.
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


namespace VSBDS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // only creates new db if not created yet
            bool dbcreated = true;
            try
            {
                StreamReader read = new StreamReader("sceneindex.txt");
            }
            catch (FileNotFoundException EE)
            {
                Console.WriteLine("The file sceneindex.txt does not exist");
                dbcreated = false;
            }
            if (!dbcreated)
            {
                createVideoDB();
            }

            InitializeComponent();
            LoadImages();
            // run the timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timerTick;
            timer.Start();
        }
    }
}
