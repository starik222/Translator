using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Translator.IO;
using System.IO;

namespace MinkTools
{
    public class BMPFile
    {
        public enum BitmapCompressionMode : uint
        {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct BITMAPFILEHEADER
        {
            public ushort bfType;
            public uint bfSize;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfOffBits;

            public void Init()
            {
                bfType = 19778;
                bfReserved1 = 0;
                bfReserved2 = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public BitmapCompressionMode biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;

            public void Init()
            {
                biSize = (uint)Marshal.SizeOf(this);
                biPlanes = 1;
                biCompression = BitmapCompressionMode.BI_RGB;
                biClrUsed = 0;
                biClrImportant = 0;
                biXPelsPerMeter = 0;
                biYPelsPerMeter = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }


        public BITMAPFILEHEADER bmf;
        public BITMAPINFOHEADER bmi;

        public byte[] imageData;

        public BMPFile(int width, int height, int bitCount)
        {
            bmi.Init();
            bmf.Init();
            bmf.bfOffBits = 54;
            bmi.biWidth = width;
            bmi.biHeight = height;
            bmi.biBitCount = (ushort)bitCount;
        }

        public BMPFile()
        {
        }

        public void SaveImage(string FilePath, byte[] imgBuffer)
        {
            FileStream fs = new FileStream(FilePath, FileMode.Create);
            CustomBinaryWriter writer = new CustomBinaryWriter(fs);
            bmi.biSizeImage = (uint)imgBuffer.Length;
            writer.WriteRawSerialize(bmf);
            writer.WriteRawSerialize(bmi);
            writer.Write(imgBuffer);
            writer.Close();
        }

        public void LoadImage(string FilePath)
        {
            FileStream fs = new FileStream(FilePath, FileMode.Open);
            CustomBinaryReader reader = new CustomBinaryReader(fs);
            bmf = (BITMAPFILEHEADER)reader.ReadStruct(typeof(BITMAPFILEHEADER));
            bmi = (BITMAPINFOHEADER)reader.ReadStruct(typeof(BITMAPINFOHEADER));
            imageData = reader.ReadToEnd();
        }
    }
}
