using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator.Compression
{
    internal class LzWindowDictionary
    {
        private int BlockSize = 0;
        private int MaxMatchAmount = 0x12;
        private int MinMatchAmount = 3;
        private List<int>[] OffsetList = new List<int>[0x100];
        private int WindowLength = 0;
        private int WindowSize = 0x1000;
        private int WindowStart = 0;

        public LzWindowDictionary()
        {
            for (int i = 0; i < this.OffsetList.Length; i++)
            {
                this.OffsetList[i] = new List<int>();
            }
        }

        public void AddEntry(byte[] DecompressedData, int offset)
        {
            this.OffsetList[DecompressedData[offset]].Add(offset);
        }

        public void AddEntryRange(byte[] DecompressedData, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                this.AddEntry(DecompressedData, offset + i);
            }
        }

        private void RemoveOldEntries(byte index)
        {
            int num = 0;
            while (num < this.OffsetList[index].Count)
            {
                if (this.OffsetList[index][num] >= this.WindowStart)
                {
                    break;
                }
                this.OffsetList[index].RemoveAt(0);
            }
        }

        public int[] Search(byte[] DecompressedData, uint offset, uint length)
        {
            this.RemoveOldEntries(DecompressedData[offset]);
            if ((offset < this.MinMatchAmount) || ((length - offset) < this.MinMatchAmount))
            {
                return new int[2];
            }
            int[] numArray = new int[2];
            for (int i = this.OffsetList[DecompressedData[offset]].Count - 1; i >= 0; i--)
            {
                int num = this.OffsetList[DecompressedData[offset]][i];
                int num2 = 1;
                while ((((num2 < this.MaxMatchAmount) && (num2 < this.WindowLength)) && (((num + num2) < offset) && ((offset + num2) < length))) && (DecompressedData[(int)((IntPtr)(offset + num2))] == DecompressedData[num + num2]))
                {
                    num2++;
                }
                if ((num2 >= this.MinMatchAmount) && (num2 > numArray[1]))
                {
                    numArray = new int[] { ((int)offset) - num, num2 };
                    if (num2 == this.MaxMatchAmount)
                    {
                        return numArray;
                    }
                }
            }
            return numArray;
        }

        public void SetBlockSize(int size)
        {
            this.BlockSize = size;
            this.WindowLength = size;
        }

        public void SetMaxMatchAmount(int amount)
        {
            this.MaxMatchAmount = amount;
        }

        public void SetMinMatchAmount(int amount)
        {
            this.MinMatchAmount = amount;
        }

        public void SetWindowSize(int size)
        {
            this.WindowSize = size;
        }

        public void SlideBlock()
        {
            this.WindowStart += this.BlockSize;
        }

        public void SlideWindow(int Amount)
        {
            if (this.WindowLength == this.WindowSize)
            {
                this.WindowStart += Amount;
            }
            else if ((this.WindowLength + Amount) <= this.WindowSize)
            {
                this.WindowLength += Amount;
            }
            else
            {
                Amount -= this.WindowSize - this.WindowLength;
                this.WindowLength = this.WindowSize;
                this.WindowStart += Amount;
            }
        }
    }
}
