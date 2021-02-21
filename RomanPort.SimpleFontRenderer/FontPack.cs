using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RomanPort.SimpleFontRenderer
{
    public class FontPack
    {
        public byte width;
        public byte height;
        public Dictionary<char, float[]> font;

        public const int HEADER_SIZE = 8;
        public const int CHAR_HEADER_SIZE = 2;

        public FontPack(byte width, byte height)
        {
            this.width = width;
            this.height = height;
            font = new Dictionary<char, float[]>();
        }

        public int MeasureWidth(int stringLen)
        {
            return width * stringLen;
        }

        public static FontPack FromResource(string fullName)
        {
            FontPack pack;
            using (Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream(fullName))
                pack = FromStream(stream);
            return pack;
        }

        public static FontPack FromStream(Stream s)
        {
            //Load header
            byte[] header = new byte[Math.Max(HEADER_SIZE, CHAR_HEADER_SIZE)];
            s.Read(header, 0, HEADER_SIZE);

            //Check magic
            if (header[0] != 'S' || header[1] != 'D' || header[2] != 'R' || header[3] != 'F')
                throw new Exception("Invalid font file");

            //Read header parts
            byte version = header[4];
            byte count = header[5];
            byte width = header[6];
            byte height = header[7];

            //Create
            FontPack pack = new FontPack(width, height);

            //Load all fonts
            for (int i = 0; i < count; i++)
            {
                //Load font header
                s.Read(header, 0, CHAR_HEADER_SIZE);
                byte charCode = header[0];
                byte charFlags = header[1];

                //Read font data
                byte[] buffer = new byte[width * height];
                s.Read(buffer, 0, buffer.Length);

                //Convert font data
                float[] converted = new float[width * height];
                for (int p = 0; p < width * height; p++)
                    converted[p] = (float)buffer[p] / byte.MaxValue;
                pack.font.Add((char)charCode, converted);
            }

            return pack;
        }

        public FontPack MakeScaledPack(float hScale, float vScale)
        {
            //Make pack
            FontPack pack = new FontPack((byte)(width * hScale), (byte)(height * vScale));

            //Convert characters
            foreach (var c in font)
            {
                float[] data = new float[pack.height * pack.width];
                for (int y = 0; y < pack.height; y++)
                {
                    for (int x = 0; x < pack.width; x++)
                    {
                        //First, get data points to use on both axis
                        float yRatio = GetInterpScale(y, vScale, out int srcYa, out int srcYb);
                        float xRatio = GetInterpScale(x, hScale, out int srcXa, out int srcXb);
                        float yRatioInvert = 1 - yRatio;
                        float xRatioInvert = 1 - xRatio;

                        //Constrain
                        if (srcYa >= height)
                            srcYa = height - 1;
                        if (srcYb >= height)
                            srcYb = height - 1;

                        //Apply on the X axis
                        float p1 = (c.Value[srcXa + (srcYa * width)] * xRatioInvert) + (c.Value[srcXb + (srcYa * width)] * xRatio);
                        float p2 = (c.Value[srcXa + (srcYb * width)] * xRatioInvert) + (c.Value[srcXb + (srcYb * width)] * xRatio);

                        //Now, apply on the X axis between those two points
                        data[x + (y * pack.width)] = (p1 * yRatioInvert) + (p2 * yRatio);
                    }
                }
                pack.font.Add(c.Key, data);
            }

            return pack;
        }

        private float GetInterpScale(float x, float hScale, out int srcXa, out int srcXb)
        {
            srcXa = (int)Math.Floor(x / hScale);
            srcXb = (int)Math.Ceiling(x / hScale);
            return ((x / hScale) - srcXa) / (1 / hScale);
        }
    }
}
