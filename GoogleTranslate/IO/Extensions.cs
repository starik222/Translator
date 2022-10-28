using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Translator.IO
{
    public enum PreType
    {
        INT32,
        BYTE
    }
    public static class Extensions
    {

        public static byte[] StringToHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        public static string GetBetween(this string strSource, string strStart, string strEnd)
        {
            const int kNotFound = -1;

            var startIdx = strSource.IndexOf(strStart);
            if (startIdx != kNotFound)
            {
                startIdx += strStart.Length;
                var endIdx = strSource.IndexOf(strEnd, startIdx);
                if (endIdx > startIdx)
                {
                    return strSource.Substring(startIdx, endIdx - startIdx);
                }
            }
            return String.Empty;
        }

        public static string GetBetween(this string strSource, string strStart, string strEnd, int startIndex)
        {
            const int kNotFound = -1;

            var startIdx = strSource.IndexOf(strStart, startIndex);
            if (startIdx != kNotFound)
            {
                startIdx += strStart.Length;
                var endIdx = strSource.IndexOf(strEnd, startIdx);
                if (endIdx > startIdx)
                {
                    return strSource.Substring(startIdx, endIdx - startIdx);
                }
            }
            return String.Empty;
        }

        public static object LoadDataSet(string path)
        {
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(path));
            var obj = new BinaryFormatter().Deserialize((Stream)ms);
            ms.Close();
            return obj;
        }

        public static object LoadDataSet(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            var obj = new BinaryFormatter().Deserialize((Stream)ms);
            ms.Close();
            return obj;
        }


        public static void SaveDataSet(object lst, string path)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, lst);
            File.WriteAllBytes(path, ms.ToArray());
            ms.Close();
        }

        public static byte[] SaveDataSet(object objItem)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, objItem);
                return ms.ToArray();
            }
        }

        //public static void Write(this Stream stream, byte[] data)
        //{
        //    stream.Write(data, 0, data.Length);
        //}


        //public static object ReadStructArray(this Stream fs, Type t, long count)
        //{
        //    List<object> list = new List<object>();
        //    for (long i = 0; i < count; i++)
        //    {
        //        byte[] buffer = new byte[Marshal.SizeOf(t)];
        //        fs.Read(buffer, 0, Marshal.SizeOf(t));
        //        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        //        Object temp = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), t);
        //        handle.Free();
        //        list.Add(temp);
        //    }
        //    return list;
        //}

        public static byte[] RawSerialize(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(anything, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return rawdata;
        }

        public static Int64 ToInt64BE(byte[] data)
        {
            return data[0] << 56 | data[1] << 48 | data[2] << 40 | data[3] << 32 | data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7];
        }

        public static Int32 ToInt32BE(byte[] bytes)
        {
            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }

       /* public static byte[] GetBytesBE(this Int32 val)
        {
            byte[] temp = val.RawSerialize();
            Array.Reverse(temp);
            return temp;
        }*/

        public static byte[] GetBytesBE(Int32 val)
        {
            byte[] temp = BitConverter.GetBytes(val);
            Array.Reverse(temp);
            return temp;
        }
        public static byte[] GetBytesBE(Int64 val)
        {
            byte[] temp = BitConverter.GetBytes(val);
            Array.Reverse(temp);
            return temp;
        }

        public static byte[] GetBytesWithCount(string text, Encoding enc, int Count)
        {
            byte[] temp = new byte[Count];
            byte[] btext = enc.GetBytes(text);
            if (btext.Length < temp.Length)
            {
                for (int i = 0; i < btext.Length; i++)
                {
                    temp[i] = btext[i];
                }
            }
            else
            {
                for (int i = 0; i < temp.Length - 2; i++)
                {
                    temp[i] = btext[i];
                }
            }
            return temp;
        }
        public static string GetString0(Byte[] bt, Encoding enc)
        {
            if (bt.Length == 0)
                return string.Empty;
            if (bt.Length == 1 && bt[0] == 0x00)
                return string.Empty;
            List<byte> b = new List<byte>();
            for (int i = 0; i < bt.Length; i++)
            {
                if (bt[i] != 0x00)
                {
                    b.Add(bt[i]);
                }
                else
                {
                    break;
                }
            }
            return enc.GetString(b.ToArray());
        }

        public static byte[] GetSpecBytes(String text, int count)
        {
            byte[] res = new byte[count];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = 0x20;
            }
            if (string.IsNullOrEmpty(text))
            {
                res[0] = 0x00;
                return res;
            }
            byte[] bname = Encoding.GetEncoding(932).GetBytes(text);
            int ii = 0;
            for (ii = 0; ii < bname.Length && ii < res.Length; ii++)
            {
                res[ii] = bname[ii];
            }
            if (ii < res.Length - 1)
            {
                res[ii] = 0x00;
            }
            else
            {
                res[res.Length - 1] = 0x00;
            }
            return res;
        }

        public static byte[] CompleteBytes(Byte[] bt)
        {
            bool stop = false;
            for (int i = 0; i < bt.Length; i++)
            {
                if (stop)
                {
                    bt[i] = 0x20;
                }
                else
                {
                    if (bt[i] == 0x00)
                        stop = true;
                }
            }
            return bt;
        }

        public static byte[] FromInt32BE(int val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            byte swap = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = swap;
            swap = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = swap;
            return bytes;
        }

        public static long ArraySearch(byte[] sourceArr, byte[] FindArr)
        {
            if(FindArr==null || sourceArr == null)
                return -1;
            if (sourceArr.Length == 0 || FindArr.Length == 0 || sourceArr.Length < FindArr.Length)
                return -1;
            for (long i = 0; i < sourceArr.Length; i++)
            {
                if (sourceArr[i] == FindArr[0] && sourceArr.Length - i >= FindArr.Length)
                {
                    bool allTrue = true;
                    for (long j = 0; j < FindArr.Length; j++)
                    {
                        if (sourceArr[j + i] != FindArr[j])
                        {
                            allTrue = false;
                            break;
                        }
                    }
                    if (allTrue)
                        return i;
                }
            }
            return -1;
        }
    }
}
