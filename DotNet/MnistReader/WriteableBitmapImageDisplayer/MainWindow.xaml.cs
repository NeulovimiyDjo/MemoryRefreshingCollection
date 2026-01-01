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
using System.IO;

namespace WriteableBitmapImageDisplayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var wbmap = new WriteableBitmap(100, 100, 96, 96, PixelFormats.Gray8, null);

            this.Img.Source = wbmap;

            byte[] pixels = new byte[100 * 100];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = System.Convert.ToByte(Math.Round(255.0*i/pixels.Length));
            }

            wbmap.WritePixels(new Int32Rect(0, 0, 100, 100), pixels, 100, 0);

            byte[] image = GetImage();

            wbmap.WritePixels(new Int32Rect(20, 20, 28, 28), image, 28, 0);

        }


        static byte[] GetImage()
        {
            byte[] res = new byte[28 * 28];

            string pathImages = @"..\..\..\Mnist\t10k-images.idx3-ubyte";
            string pathLabels = @"..\..\..\Mnist\t10k-labels.idx1-ubyte";

            try
            {
                using (BinaryReader imagesReader = new BinaryReader(File.OpenRead(pathImages)), labelsReader = new BinaryReader(File.OpenRead(pathLabels)))
                {
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();

                    labelsReader.ReadInt32();
                    labelsReader.ReadInt32();

                    byte[][] pixels = new byte[28][];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = new byte[28];
                    }


                    for (int n = 0; n < 10000; n++)
                    {
                        for (int i = 0; i < 28; i++)
                        {
                            for (int j = 0; j < 28; j++)
                            {
                                pixels[i][j] = imagesReader.ReadByte();
                                if (n==2)
                                    res[i * 28 + j] = pixels[i][j];
                            }
                        }

                        byte lbl = labelsReader.ReadByte();

                    }

                }

            }
            catch (Exception e)
            {
                
            }

            return res;
        }
    }
}
