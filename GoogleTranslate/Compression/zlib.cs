using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ComponentAce.Compression.Libs.zlib;

namespace Translator.Compression
{
    public class zlibMode
    {
        public enum CompressionMode:int
        {
        Z_BEST_COMPRESSION = 9,
        Z_BEST_SPEED = 1,
        Z_DEFAULT_COMPRESSION = -1,
        Z_HUFFMAN_ONLY = 2,
        Z_NO_COMPRESSION = 0,
        }

        public static byte[] CompressData(byte[] inData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_BEST_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                return outMemoryStream.ToArray();
            }
        }

        public static byte[] CompressData(byte[] inData, CompressionMode mode)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, (int)mode))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                return outMemoryStream.ToArray();
            }
        }

        public static byte[] DecompressData(byte[] inData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                return outMemoryStream.ToArray();
            }
        }

        public static byte[] Inflate(byte[] inData)
        {
            int data = -1;
            int stopByte = -1;
            ZInputStream InStream = new ZInputStream(new MemoryStream(inData));
            MemoryStream OutStream = new MemoryStream();
            while (stopByte != (data = InStream.Read()))
            {
                byte _dataByte = (byte)data;
                OutStream.WriteByte(_dataByte);
            }
            byte[] out_buff = OutStream.ToArray();
            InStream.Close();
            OutStream.Close();
            return out_buff;
        }

        public static byte[] Deflate(byte[] inData, CompressionMode mode)
        {
            return CompressData(inData, mode);
        }

        public static byte[] InflateRaw(byte[] inData)
        {
            byte[] ModCompDataTable = new byte[inData.Length + 2];
            ModCompDataTable[0] = 0x78;
            ModCompDataTable[1] = 0x9C;
            int data = -1;
            int stopByte = -1;
            Array.Copy(inData, 0, ModCompDataTable, 2, inData.Length);
            ZInputStream InStream = new ZInputStream(new MemoryStream(ModCompDataTable));
            MemoryStream OutStream = new MemoryStream();
            while (stopByte != (data = InStream.Read()))
            {
                byte _dataByte = (byte)data;
                OutStream.WriteByte(_dataByte);
            }
            byte[] out_buff = OutStream.ToArray();
            InStream.Close();
            OutStream.Close();
            return out_buff;
        }

        public static byte[] DeflateRaw(byte[] inData, CompressionMode mode)
        {
            byte[] out_buff = CompressData(inData, mode);
            byte[] temp = new byte[out_buff.Length - 2];
            Array.Copy(out_buff, 2, temp, 0, temp.Length);
            return temp;
        }

        private static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }   
    }
}
