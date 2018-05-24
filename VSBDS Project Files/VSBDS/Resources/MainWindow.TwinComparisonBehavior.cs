/**-----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               VSDBS
 * Date Last Revision:  3/13/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the twin comparison approach of the VSDBS. It backs 
 * the MainWindow.xaml.
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
        // Arrays
        private double[,] intensityMatrix = new double[5000, 25];
        private double[] sdMatrix = new double[5000];
        private Bitmap[] FrameMatrix = new Bitmap[5000];

        // keep indices of Fs+1 and Ce
        private List<int> fs = new List<int>();
        private List<int> ce = new List<int>();

        // variables used to get all clip boundaries
        private double msd;
        private double stdsd;
        private double ssd;
        private double ts;
        private double tb;
        private int tor = 2;

        #endregion

        /* ---------------------------------------------------------------------
         * twinComparison
         * ---------------------------------------------------------------------
         * Starts the Twin Comparison Approach
         */
        private void twinComparison()
        {
            getSd();
            getStdSd();
            getScenes();
            writeCuts();
        }
        /* ---------------------------------------------------------------------
         * getIntensity
         * ---------------------------------------------------------------------
         * Gets the intensity of every pixel in all images and stores it in a 
         * histogram, called intensityMatrix. Intensity of a pixel is determined
         * by the following formula: 0.299 * red + .587 * green + 0.114 * blue
         * Preconditions:
         * -image: the image to store the color code of
         * -height: image height
         * -width: image width
         */
        private void getIntensity()
        {
            for (int image = 1000; image < 5000; image++)
            {
                Bitmap frame = FrameMatrix[image];
                int width = frame.Width;
                int height = frame.Height;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        System.Drawing.Color c = frame.GetPixel(i, j);
                        int red = c.R;
                        int green = c.G;
                        int blue = c.B;
                        double intensity = (0.299 * red) + (.587 * green) + (0.114 * blue);
                        int bucket = (int)(intensity / 10);
                        if (bucket >= intensityMatrix.GetLength(1)) bucket--;
                        intensityMatrix[image, bucket]++;
                    }
                }
            }
        }

        /* ---------------------------------------------------------------------
         * getSd
         * ---------------------------------------------------------------------
         * Calculates the difference between current frame and the next frame,
         * and stores it in the sdMatrix.
         */
        private void getSd()
        {
            // calculate sd between each frame
            double ssd = 0;
            for (int i = 1000; i < 4999; i++)
            {
                double sd = 0;
                for (int j = 0; j < 25; j++)
                {
                    sd += Math.Abs(intensityMatrix[i, j] - intensityMatrix[i + 1, j]);
                }
                sdMatrix[i] = sd;
                ssd += sd;
            }

            this.ssd = ssd;

            // get mean sd, ts, tb
            this.msd = ssd / 3999;
            this.ts = this.msd * 2;
            getStdSd();
            this.tb = msd + (this.stdsd * 11);
        }

        /* ---------------------------------------------------------------------
         * getStdSd
         * ---------------------------------------------------------------------
         * Calculates the standard deviation of all SDs and stores it.
         */
        private void getStdSd()
        {
            double se = 0;
            for (int i = 1000; i < 4999; i++)
            {
                double difference = sdMatrix[i] - this.msd;
                se += difference * difference;
            }
            this.stdsd = Math.Sqrt(se / (3999 - 1));
        }


        /* ---------------------------------------------------------------------
         * getScenes
         * ---------------------------------------------------------------------
         * Returns true if the SD at the given index is greater than Ts.
         * Preconditions:
         *  -fsCandi within the range of 1000 to 4999
         */
         private void getScenes()
        {
            int sceneStart = 1000;
            int sceneEnd = 1000;
            int fsCandi = -1;
            int feCandi = -1;

            for (int i = 1000; i < 4999; i++)
            {
                // if cut, then store front and end pair, store cut index
                if (isCut(i))
                {
                    sceneEnd = i - 1;
                    clipIndicies.Add(sceneStart, sceneEnd);
                    ce.Add(sceneEnd);
                    sceneStart = i;
                    continue;
                }

                else if (isFsCandi(i))
                {
                    fsCandi = i;
                    int offset = 0;
                    while (true)
                    {
                        if (isFeCandi(i + offset))
                        {
                            feCandi = i + offset;
                            //if is transition, scene end is fsCandi
                            if (isTransition(fsCandi, feCandi))
                            {
                                sceneEnd = fsCandi - 1;
                                clipIndicies.Add(sceneStart, sceneEnd);
                                fs.Add(sceneEnd);

                                // set fs + 1 to be the next start
                                sceneStart = fsCandi;

                                // move i forward to after the transition
                                i = feCandi;
                            }
                            break;
                        }
                        offset++;
                    }
                }
                continue;
            }

            if (sceneEnd != 4999)
            {
                sceneEnd = 4999;
                clipIndicies.Add(sceneStart, sceneEnd);
            }
         }

        

        /* ---------------------------------------------------------------------
         * isFsCandi
         * ---------------------------------------------------------------------
         * Returns true if the SD at the given index is greater than Ts.
         * Preconditions:
         *  -fsCandi within the range of 1000 to 4999
         */
        private bool isFsCandi(int indexSD)
        {
            return (sdMatrix[indexSD] >= ts);
        }



        /* ---------------------------------------------------------------------
         * isFeCandi
         * ---------------------------------------------------------------------
         * Returns true if the SD at the given index can be an feCandi. To be an
         * feCandi, the next two SD must be below Ts or reaches a cut boundary
         * is greater than or equal to TB.
         * Preconditions:
         *  -fsCandi and feCandi are within the range of 1000 to 4999
         */
        private bool isFeCandi(int indexSD)
        {
            if (sdMatrix[indexSD + 1] > tb)
            {
                return true;
            }
            if (indexSD == 4998 && sdMatrix[indexSD] > ts)
            {
                return true;
            }
            if (indexSD == 4997 && sdMatrix[indexSD + 1] < ts)
            {
                return true;
            }
            if (sdMatrix[indexSD + 1] < ts && sdMatrix[indexSD + 2] < ts)
            {
                return true;
            }
            return false;
        }

        /* ---------------------------------------------------------------------
         * isCut
         * ---------------------------------------------------------------------
         * Returns true if the cut is a boundary, where the SD >= Tb at the
         * given index.
         */
        private bool isCut(int indexSD)
        {
            return (sdMatrix[indexSD] >= tb);
        }

        /* ---------------------------------------------------------------------
         * isTransition
         * ---------------------------------------------------------------------
         * Returns true if the sum SD between fsCandi and feCandi
         * is greater than or equal to TB.
         * Preconditions:
         *  -fsCandi and feCandi are within the range of 1000 to 4999
         */
        private bool isTransition(int fsCandi, int feCandi)
        {
            double totalSD = 0;
            for (int i = fsCandi; i <= feCandi; i++)
            {
                totalSD += sdMatrix[i];
            }
            return (totalSD >= tb);
        }

        /* ---------------------------------------------------------------------
         * writeCuts
         * ---------------------------------------------------------------------
         * Writes all indices of ce and fs+1 into a file.
         */
        private void writeCuts()
        {
            StreamWriter writer = new StreamWriter("cuts.txt");

            // name the file
            writer.WriteLine("Fs+1 indices:");
            foreach (int index in fs)
            {
                writer.WriteLine(" " + index);
            }

            writer.WriteLine("Ce indices:");
            foreach (int index in ce)
            {
                writer.WriteLine(" " + index);
            }
            writer.Close();
        }
    }
}
