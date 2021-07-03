using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Translator.IO
{
    public class CustomBinaryWriter : BinaryWriter
    {

        public CustomBinaryWriter(Stream output) : base(output) { }
        public CustomBinaryWriter(Stream output, Encoding encoding) : base(output, encoding) { }



        public void WriteRawSerialize(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(anything, handle.AddrOfPinnedObject(), false);
            handle.Free();
            Write(rawdata);
        }


        public void WriteInt32BE(int val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            byte swap = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = swap;
            swap = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = swap;
            Write(bytes);
        }

        public void WriteDateTime(DateTime date)
        {
            Write(date.ToBinary());
        }

        public void WriteStringWithPreLenBE(string text, Encoding encoding)
        {
            byte[] textbuf = encoding.GetBytes(text);
            WriteInt32BE(textbuf.Length);
            Write(textbuf);
            Write((byte)0x00);
        }

        public void WriteStringWithPreLen(string text, Encoding encoding, PreType type, bool EndZero)
        {
            byte[] textbuf = encoding.GetBytes(text);
            if (type == PreType.INT32)
                Write(textbuf.Length);
            else if(type == PreType.BYTE)
                Write((byte)textbuf.Length);
            Write(textbuf);
            if (EndZero)
            {
                Write((byte)0x00);
                if (encoding == Encoding.Unicode)
                    Write((byte)0x00);
            }
        }

        public void WriteUnicodeString0(string s)
        {
            Write(Encoding.Unicode.GetBytes(s));
            Write((byte)0);
            Write((byte)0);
        }

        public void WriteXORString0(string text, Encoding encoding)
        {
            if (text == null)
                text = string.Empty;
            byte[] buf = encoding.GetBytes(text);
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] ^= 0xFF;
            }
            Write(buf);
            Write((byte)0xFF);
            int align4 = (int)((BaseStream.Position & 3) > 0 ? 4 - (BaseStream.Position & 3) : 0);
            for (int i = 0; i < align4; i++)
            {
                Write((byte)0xFF);
            }

        }

        public void WriteString0(string s, Encoding encoding)
        {
            Write(encoding.GetBytes(s));
            Write((byte)0);
        }
        public void WriteString(string s, Encoding encoding)
        {
            Write(encoding.GetBytes(s));
        }

        public void WriteString(string s, Encoding encoding, int len)
        {
            byte[] btext = encoding.GetBytes(s);
            if (len == btext.Length)
                Write(btext);
            else if (len < btext.Length)
            {
                for (int i = 0; i < len; i++)
                {
                    Write(btext[i]);
                }
            }
            else
            {
                int raz = len - btext.Length;
                Write(btext);
                for (int i = 0; i < raz; i++)
                {
                    Write((byte)0x00);
                }
            }
        }
        public void WriteName0(string s)
        {
            Write(System.Text.UTF8Encoding.UTF8.GetBytes(s));
            Write((byte)0);
        }

        public void WriteNameA4(string name)
        {
            /*byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(name);
            writer.Write(bytes.Length);
            writer.Write(bytes);
            if ((name.Length & 3) != 0)
            {
                writer.Write(new byte[4 - (name.Length & 3)]);
            }*/
            WriteNameA4U8(name);
        }

        public void WriteNameA4U8(string name)
        {
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(name);
            Write(bytes.Length);
            Write(bytes);
            if ((bytes.Length & 3) != 0)
            {
                BaseStream.Position += 4 - (bytes.Length & 3);
            }
        }

        static void WriteArray<T>(Action<T> del, T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                del(array[i]);
            }
        }

        public void Write(float[] array)
        {
            WriteArray<float>(new Action<float>(Write), array);
        }

        public void Write(ushort[] array)
        {
            WriteArray<ushort>(new Action<ushort>(Write), array);
        }

        public void Write(int[] array)
        {
            WriteArray<int>(new Action<int>(Write), array);
        }

        public void Write(uint[] array)
        {
            WriteArray<uint>(new Action<uint>(Write), array);
        }

        public void Write(SByte[] array)
        {
            WriteArray<SByte>(new Action<SByte>(Write), array);
        }
    }
}
