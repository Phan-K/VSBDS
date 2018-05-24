/**-----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               VSDBS
 * Date Last Revision:  3/13/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the video handling of the VSDBS. It backs the
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

namespace VSBDS
{
    public partial class MainWindow : Window
    {
        #region Variables

        // stores the beginning and end indices of each clip
        private SortedDictionary<int, int> clipIndicies = new SortedDictionary<int, int>();

        //video information to be used
        private int vWidth;
        private int vHeight;
        private int vFramerate;

        #endregion

        /* ---------------------------------------------------------------------
         * createVideoDB
         * ---------------------------------------------------------------------
         * Creates the video database. Reads the initial video, cuts it up into
         * scenes, turns the scenes into videos, saving each scene into a 
         * separate video.
         */
        private void createVideoDB()
        {
            readVideo();
            twinComparison();
            writeVideos();
            moveAviFiles(); 
        }

        /* ---------------------------------------------------------------------
         * readVideo
         * ---------------------------------------------------------------------
         * Reads the video, saving all its frames and creating the intensity
         * matrix for each one.
         * Preconditions: 
         *  -video of specified name exists
         */
        private void readVideo()
        {
            String VideoPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            VideoPath = VideoPath + "\\Resources\\20020924_juve_dk_02a.avi";
            bool intensityExists = readIntensityFile();
            using (var vFReader = new VideoFileReader())
            {
                vFReader.Open(VideoPath);

                // get video info to remake video when done
                this.vWidth = vFReader.Width;
                this.vHeight = vFReader.Height;
                this.vFramerate = vFReader.FrameRate;
                
                // store each image and intensities into the arrays
                for (int i = 1000; i < 5000; i++)
                {
                    System.Drawing.Bitmap bmpBaseOriginal = vFReader.ReadVideoFrame();
                    FrameMatrix[i] = bmpBaseOriginal;
                }
                vFReader.Close();

                if (!intensityExists)
                {
                    getIntensity();
                    writeIntensityFile();
                }
            }
        }

        private bool readIntensityFile()
        {
            StreamReader read;
            string line;
            int lineNumber = 1000;
            try
            {
                read = new StreamReader("intensity.txt");
                while ((line = read.ReadLine()) != null)
                {
                    String[] substrings = line.Split(' ');
                    for (int i = 0; i < substrings.Length - 1; i++)
                    {
                        int substringlength = substrings.Length - 1;
                        intensityMatrix[lineNumber, i] =
                            (double)Int32.Parse(substrings[i]);
                    }
                    lineNumber++;
                }
                read.Close();
                return true;
            }
            catch (FileNotFoundException EE)
            {
                Console.WriteLine("The file intensity.txt does not exist");
                return false;
            }
        }


        /* ----------------------------------------------------------------------
         * writeIntensityFile
         * ----------------------------------------------------------------------
         * This method writes the contents of the specified 2D array to a file 
         * of the given name.
         * Preconditions:
         *  -name is a string of a valid filename
         *  -matrix is valid
         */
        private void writeIntensityFile()
        {
            StreamWriter writer = new StreamWriter("intensity.txt");
            for (int i = 1000; i < 5000; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    writer.Write((int)intensityMatrix[i, j]);
                    writer.Write(" ");
                }
                writer.WriteLine();
            }
            writer.Close();
        }

        /* ---------------------------------------------------------------------
         * writeVideos
         * ---------------------------------------------------------------------
         * Publishes each pair of start-end frames for the scenes stored in 
         * dictinoary as a video, outputs these pairs to a file.
         */
        private void writeVideos()
        {
            StreamWriter writer = new StreamWriter("sceneindex.txt");
            int videoNo = 0;
            foreach (int key in clipIndicies.Keys)
            {
                // name the file
                String fileno = "";
                if (videoNo < 10) fileno += "000";
                else if (videoNo < 100) fileno += "00";
                else if (videoNo < 1000) fileno += "0";
                fileno += videoNo;
                Console.WriteLine("Creating video " + fileno);
                writeVideo(key, clipIndicies[key], fileno);
                writer.WriteLine("Scene " + videoNo + ": " + key + ", " +
                    clipIndicies[key]);
                videoNo++;
            }
            writer.Close();
        }

        /* ---------------------------------------------------------------------
         * writeVideo
         * ---------------------------------------------------------------------
         * Outputs a video using frames from frameMatrix given start and end 
         * frame indices. Also saves the first frame as a jpg image for a 
         * thumbnail.
         * Preconditions:
         *  -start <= end
         *  -1000 <= start <= end <= 4999
         */
        private void writeVideo(int start, int end, string fileno)
        {
            // save the video
            String vFilename = (fileno + ".avi");
            VideoFileWriter writer = new VideoFileWriter();
            writer.Open(vFilename, this.vWidth, this.vHeight, this.vFramerate, 
                VideoCodec.MPEG4);
            for (int i = start; i <= end; i++)
            {
                writer.WriteVideoFrame(FrameMatrix[i]);
            }
            writer.Close();

            // save the first frame as a jpg thumbnail
            String pFilename = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            pFilename += "\\Resources\\Image\\" + fileno + ".jpg";
            FrameMatrix[start].Save(pFilename,
                System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        /* ---------------------------------------------------------------------
         * moveAviFiles
         * ---------------------------------------------------------------------
         * Moves all created .avi files from \bin\Debug to Resources\Video  
         * folder for later use.
         */
        private void moveAviFiles()
        {
            String WritePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            String SourcePath = WritePath + "\\bin\\Debug\\";
            String DestinationPath = WritePath + "\\Resources\\Video\\";
            DirectoryInfo d = new DirectoryInfo(SourcePath);
            foreach (var file in d.GetFiles("*.avi"))
            {
                File.Move(file.FullName, DestinationPath + file.Name);
            }
        }
    }
}
