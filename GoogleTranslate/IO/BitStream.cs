using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Translator.IO
{
    public class BitStream
    {
        private MemoryStream baseStream;
        private int bitPos;
        private int preBuf;
        private int bytesOut;
        private bool eof;
        public BitStream()
        {
            eof = false;
            baseStream = new MemoryStream();
            bitPos = 0;
            preBuf = 0;
            bytesOut = 0;
        }

        public BitStream(byte[] data)
        {
            eof = false;
            baseStream = new MemoryStream(data);
            bitPos = 0;
            preBuf = 0;
            bytesOut = 0;
        }

        public bool EndOfStream()
        {
            return eof;
            //if (baseStream.Position > baseStream.Length)
            //    return true;
            //else
            //    return false;
        }

        public byte[] ToArray()
        {
            return baseStream.ToArray();
        }

        public int ReadBits(int bitCount)
        {
            while (bitPos < bitCount)
            {
                int b = baseStream.ReadByte();
                if (/*b == null ||*/ b == -1)
                {
                    eof = true;
                }
                preBuf = (preBuf << 8) | (b & 0xFF); //fill buffer
                bitPos += 8;
            }

            int v = (preBuf >> (bitPos - bitCount)) & ((1 << bitCount) - 1);
            bitPos -= bitCount;
            return v;
        }

        public bool ReadBit()
        {
            return Convert.ToBoolean(ReadBits(1));
        }

        public byte ReadByte()
        {
            return (byte)ReadBits(8);
        }


        public void WriteBits(int value, int bitCount)
        {
            while (bitPos >= 8)
            {
                int ch = (preBuf >> 24);
                unchecked { baseStream.WriteByte((byte)ch); } // write 8-bit
                preBuf <<= 8;
                bitPos -= 8;
                ++bytesOut;
            }
            preBuf |= (value << (32 - bitPos - bitCount));
            bitPos += bitCount;
        }

        public void FinishWrite()
        {
            while (bitPos > 0)
            {
                int ch = (preBuf >> 24);
                baseStream.WriteByte((byte)ch); // write 8-bit
                preBuf <<= 8;
                bitPos -= 8;
                bytesOut++;
            }
        }
        public void WriteByte(byte value)
        {
            WriteBits(value, 8);
        }



    }
}
