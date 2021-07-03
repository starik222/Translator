using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Translator.IO
{
    public class CustomBinaryReader : BinaryReader
    {

        public CustomBinaryReader(Stream input) : base(input) { }
        public CustomBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }


        public object ReadStruct(Type t)
        {
            byte[] buffer = new byte[Marshal.SizeOf(t)];
            BaseStream.Read(buffer, 0, Marshal.SizeOf(t));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Object temp = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), t);
            handle.Free();
            return temp;
        }

        public Int32 ReadInt32BE()
        {
            byte[] bytes = ReadBytes(4);
            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }

        public byte[] ReadToEnd()
        {
            long size = BaseStream.Length - BaseStream.Position;
            if (size == 0)
                return null;
            else if (size < 0)
                throw new OutOfMemoryException("Размер буфера не может быть отрицателен");
            return ReadBytes((int)size);
        }

        public string ReadName0()
        {
            byte[] buffer = new byte[100];
            int len = 0;
            for (; (buffer[len] = ReadByte()) != 0; len++)
            {
            }
            return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, len);
        }

        public string ReadString(int len)
        {
            byte[] buffer = new byte[len];
            buffer = ReadBytes(len);
            return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, len);
        }

        public string ReadLine(Encoding encoding)
        {
            List<byte> array = new List<byte>();
            byte b = 0;
            while (this.BaseStream.Position < this.BaseStream.Length && (b = ReadByte())!=0x0A)
            {
                array.Add(b);
            }
            if (array[array.Count - 1] == 0x0d)
                array.RemoveAt(array.Count - 1);
            return encoding.GetString(array.ToArray());
        }
        public string ReadLine()
        {
            return ReadLine(Encoding.UTF8);
        }

        public DateTime ReadDateTime()
        {
            return DateTime.FromBinary(ReadInt64());
        }

        public string ReadString(int len, Encoding encoding)
        {
            byte[] buffer = new byte[len];
            buffer = ReadBytes(len);
            return encoding.GetString(buffer, 0, len);
        }

        public string ReadString(int len, Encoding encoding, bool CutZero)
        {
            byte[] buffer = new byte[len];
            buffer = ReadBytes(len);
            string text =  encoding.GetString(buffer, 0, len);
            if (CutZero)
            {
                int zpos = -1;
                if ((zpos = text.IndexOf('\0')) != -1)
                {
                    return text.Substring(0, zpos);
                }
            }
            return text;
            //return 
        }

        public string ReadXORString0(Encoding encoding)
        {
            List<byte> buf = new List<byte>();
            while (true)
            {
                byte b = ReadByte();
                if (b == 0xFF)
                    break;
                b ^= 0xFF;
                buf.Add(b);
            }
            return encoding.GetString(buf.ToArray());
        }

        public string ReadString0(Encoding encoding)
        {
            List<byte> buf = new List<byte>();
            while (true)
            {
                byte b = ReadByte();
                if (b == 0x00)
                    break;
                buf.Add(b);
            }
            return encoding.GetString(buf.ToArray());
        }

        public string ReadStringWithPreLenBE(Encoding encoding)
        {
            int TextLen = ReadInt32BE();
            byte[] buff = ReadBytes(TextLen);
            ReadByte();
            return encoding.GetString(buff);
        }

        public string ReadStringWithPreLen(Encoding encoding, PreType type, bool EndZero)
        {
            int TextLen = -1;
            if(type== PreType.INT32)
                TextLen = ReadInt32();
            else if(type == PreType.BYTE)
                TextLen = ReadByte();
            byte[] buff = ReadBytes(TextLen);
            if (EndZero)
            {
                ReadByte();
                if (encoding == Encoding.Unicode)
                    ReadByte();
            }
            return encoding.GetString(buff);
        }

        public string ReadUnicodeString0()
        {
            List<byte> buf = new List<byte>();
            while (true)
            {
                byte a = ReadByte();
                byte b = ReadByte();
                if (a == 0x00 && b == 0x00)
                    break;
                buf.Add(a);
                buf.Add(b);
            }
            return Encoding.Unicode.GetString(buf.ToArray());
        }

        public string ReadNameA4()
        {
            /*int len = reader.ReadInt32();
            int align4 = (len & 3) > 0 ? 4 - (len & 3) : 0;
            byte[] buffer = reader.ReadBytes(len + align4);
            return System.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, len);*/
            return ReadNameA4U8();
        }

        public byte[] ReadReverseRGBA()
        {
            byte[] bb = new byte[4];
            bb = ReadBytes(4);
            byte r = bb[0];
            bb[0] = bb[2];
            bb[2] = r;
            return bb;
        }

        public string ReadNameA4U8()
        {
            int len = ReadInt32();
            int align4 = (len & 3) > 0 ? 4 - (len & 3) : 0;
            byte[] buffer = ReadBytes(len + align4);
            return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, len);
        }

        public string ReadNameA4U8(out int align4)
        {
            int len = ReadInt32();
            align4 = (len & 3) > 0 ? 4 - (len & 3) : 0;
            byte[] buffer = ReadBytes(len + align4);
            return System.Text.UTF8Encoding.UTF8.GetString(buffer, 0, len);
        }

        static T[] ReadArray<T>(BinaryReader reader, Func<T> del, int length)
        {
            T[] array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = del();
            }
            return array;
        }

        public float[] ReadSingleArray(int length)
        {
            return ReadArray<float>(this, new Func<float>(ReadSingle), length);
        }

        public ushort[] ReadUInt16Array(int length)
        {
            return ReadArray<ushort>(this, new Func<ushort>(ReadUInt16), length);
        }

        public int[] ReadInt32Array(int length)
        {
            return ReadArray<int>(this, new Func<int>(ReadInt32), length);
        }

        public uint[] ReadUInt32Array(int length)
        {
            return ReadArray<uint>(this, new Func<uint>(ReadUInt32), length);
        }

        public byte[] ReadBytes(uint count)
        {
            return ReadBytes((int)count);
        }

        public sbyte[] ReadSBytes(int count)
        {
            return ReadArray<SByte>(this, new Func<SByte>(ReadSByte), count);
        }
    }
}
