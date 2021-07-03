using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Translator.IO;

namespace Translator.Compression
{
    public class HuffmanEncoder
    {

        private TreeNode rootNode;
        private List<TreeNode> nodes;
        private BitStream stream;
        private Symbol[] symbols;

        public HuffmanEncoder()
        {
            symbols = new Symbol[256];
            nodes = new List<TreeNode>();
            rootNode = new TreeNode();
        }



        public byte[] Decompress(byte[] data, int origLen)
        {
            stream = new BitStream(data);
            MemoryStream outBuffer = new MemoryStream();
            rootNode = RestoreTree(rootNode);
            TreeNode node = new TreeNode();
            while (!stream.EndOfStream())
            {
                node = rootNode;
                while (node.Symbol < 0)
                {
                    if (stream.ReadBit())
                        node = node.ChildB;
                    else
                        node = node.ChildA;
                    if (stream.EndOfStream())
                        return GetBuffer(outBuffer, origLen);
                }
                outBuffer.WriteByte((byte)node.Symbol);

            }
            return GetBuffer(outBuffer, origLen);

        }
        public byte[] GetBuffer(MemoryStream stream, int len)
        {
            byte[] data = stream.ToArray();
            byte[] ndata = new byte[len];
            if (data.Length > len)
            {
                Array.Copy(data, ndata, len);
                return ndata;
            }
            else
                return data;
        }

        public byte[] Compress(byte[] data)
        {
            stream = new BitStream();
            MakeTree(data);
            storeTree(rootNode, 0, 0);

            for (int i = 0; i < data.Length; i++)
            {
                stream.WriteBits(symbols[data[i]].code, symbols[data[i]].bits);
            }
            stream.FinishWrite();
            return stream.ToArray();
        }

        //Записываем дерево в память
        private void storeTree(TreeNode node, int code, int bits)
        {
            if (node.Symbol >= 0)
            {
                stream.WriteBits(0, 1);
                stream.WriteByte((byte)node.Symbol);
                symbols[(byte)node.Symbol].code = code;
                symbols[(byte)node.Symbol].bits = bits;
                return;
            }
            else
            {
                stream.WriteBits(1, 1);
            }

            storeTree(node.ChildA, (code << 1) + 0, bits + 1);
            storeTree(node.ChildB, (code << 1) + 1, bits + 1);
        }

        //Создаем дерево
        public void MakeTree(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (symbols[data[i]] == null)
                    symbols[data[i]] = new Symbol();
                symbols[data[i]].count++;
                symbols[data[i]].symbol = data[i];
            }

            foreach (var symbol in symbols)
            {
                if (symbol!= null && symbol.count > 0)
                    nodes.Add(new TreeNode() { Symbol = symbol.symbol, Count = symbol.count });
            }

            while (nodes.Count > 1)
            {
                List<TreeNode> orderedNodes = nodes.OrderBy(node => node.Count).ToList<TreeNode>();

                if (orderedNodes.Count >= 2)
                {
                    List<TreeNode> taken = orderedNodes.Take(2).ToList<TreeNode>();
                    TreeNode parent = new TreeNode()
                    {
                        Symbol = -1,
                        Count = taken[0].Count + taken[1].Count,
                        ChildA = taken[0],
                        ChildB = taken[1]
                    };

                    nodes.Remove(taken[0]);
                    nodes.Remove(taken[1]);
                    nodes.Add(parent);
                }

                rootNode = nodes.FirstOrDefault();
            }
        }

        public TreeNode RestoreTree(TreeNode node)
        {
            TreeNode thisNode = new TreeNode();
            if (stream.ReadBit())
            {
                thisNode.ChildA = RestoreTree(node);
                thisNode.ChildB = RestoreTree(node);
                return thisNode;
            }
            else
            {
                thisNode.Symbol = stream.ReadByte();
                return thisNode;
            }
        }



        public class TreeNode
        {
            public TreeNode ChildA;
            public TreeNode ChildB;
            public int Count;
            public int Symbol;

            public TreeNode()
            {
                ChildA = null;
                ChildB = null;
                Count = 0;
                Symbol = -1;
            }
        }

        public class Symbol
        {
            public byte symbol;
            public int count;
            public int code;
            public int bits;
        }
    }
}
