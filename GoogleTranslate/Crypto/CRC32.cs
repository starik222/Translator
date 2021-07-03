using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator.Crypto
{
    public class CRC32
    {
        public uint[] crc32tableV1 = new uint[256];
        public uint[] crc32tableV2 = new uint[256];
        public const uint INITIAL_VALUE = 0xffffffff;

        public enum GenMethod
        {
            First,
            Second
        }
        public CRC32()
        {
            InitTableV1();
            InitTableV2();
        }

        private void InitTableV1()
        {
            uint POLYNOMIAL = 0x4C11DB7;
            uint value = 0;
            for (uint i = 0; i < 256; i++)
            {
                value = i << 24;
                for (int j = 0; j < 8; j++)
                {
                    if ((value & 0x80000000L) != 0)
                    {
                        value = (value << 1) ^ POLYNOMIAL;
                    }
                    else
                    {
                        value = (value << 1);
                    }
                }
                crc32tableV1[i] = value;
            }
        }

        private void InitTableV2()
        {
            uint POLYNOMIAL = 0xEDB88320;
            uint value = 0;
            uint j = 0;
            for (uint i = 0; i < 256; i++)
            {
                for (value = i, j = 8; j > 0; j--)
                {
                    if ((value & 1) != 0)
                    {
                        value = (value >> 1) ^ POLYNOMIAL;
                    }
                    else
                    {
                        value = value >> 1;
                    }

                }
                crc32tableV2[i] = value;
            }
        }

        private uint GenerateCRC32V1(byte[] buffer, uint crc_init_val)
        {
            uint j = 0;
            for (uint i = 0; i < buffer.Length; i++)
            {
                j = ((crc_init_val >> 24) ^ buffer[i]) & 0xFF;
                crc_init_val = (crc_init_val << 8) ^ crc32tableV1[j];
            }
            return crc_init_val ^ 0xFFFFFFFF;
        }

        private uint GenerateCRC32V2(byte[] buffer, uint crc_init_val)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                crc_init_val = (crc_init_val >> 8) ^ crc32tableV2[(crc_init_val ^ buffer[i]) & 0xff];
            }
            return crc_init_val ^ 0xFFFFFFFF;
        }

        public uint GenerateCRC32(byte[] buffer, GenMethod Method)
        {
            if (Method == GenMethod.First)
            {
                return GenerateCRC32V1(buffer, INITIAL_VALUE);
            }
            else if (Method == GenMethod.Second)
            {
                return GenerateCRC32V2(buffer, INITIAL_VALUE);
            }
            else
                return 0;
        }


    }
}
