using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MnistBackpropNetwork
{
    class MnistDataReader
    {
        public byte[][] ReadImages(string path, int size)
        {
            byte[][] res = new byte[size][];

            try
            {
                using (BinaryReader imagesReader = new BinaryReader(File.OpenRead(path)))
                {
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();
                    imagesReader.ReadInt32();


                    for (int n = 0; n < size; n++)
                    {
                        res[n] = new byte[28 * 28];

                        for (int i = 0; i < 28; i++)
                        {
                            for (int j = 0; j < 28; j++)
                            {
                                res[n][i * 28 + j] = imagesReader.ReadByte();
                            }
                        }

                    }

                }

            }
            catch (Exception e)
            {

            }

            return res;
        }

        public byte[] ReadLabels(string path, int size)
        {
            byte[] res = new byte[size];

            try
            {
                using (BinaryReader labelsReader = new BinaryReader(File.OpenRead(path)))
                {
                    labelsReader.ReadInt32();
                    labelsReader.ReadInt32();

                    for (int n = 0; n < size; n++)
                    {
                        res[n] = labelsReader.ReadByte();
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
