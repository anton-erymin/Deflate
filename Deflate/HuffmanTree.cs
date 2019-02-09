using System;
using System.Collections.Generic;
using System.Text;

namespace Deflate
{
    class HuffmanTree
    {
        public TreeNode root;
        public List<TreeNode> leaves;


        public void BuildHuffmanTree(byte[] lengths)
        {
            leaves = new List<TreeNode>();

            byte maxLen = 0;

            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] != 0)
                {
                    TreeNode node = new TreeNode();
                    node.value = i;
                    node.codeLength = lengths[i];
                    leaves.Add(node);
                }

                if (lengths[i] > maxLen)
                    maxLen = lengths[i];
            }

            int[] blcount = new int[maxLen + 1];
            int[] nextCode = new int[maxLen + 1];

            for (int i = 0; i < blcount.Length; i++)
                blcount[i] = 0;

            for (int i = 0; i < lengths.Length; i++)
                blcount[lengths[i]]++;

            int code = 0;
            blcount[0] = 0;
            for (int bits = 1; bits <= maxLen; bits++)
            {
                code = (code + blcount[bits - 1]) << 1;
                nextCode[bits] = code;
            }

            for (int i = 0; i < leaves.Count; i++)
            {
                byte len = leaves[i].codeLength;
                leaves[i].huffmanCode = nextCode[len];
                nextCode[len]++;
            }

            MakeLinks();
        }


        public void BuildFixedHuffmanTree()
        {
            // Строим фиксированное дерево Хаффмана

            leaves = new List<TreeNode>(288);

            for (int i = 0; i < 288; i++)
            {
                TreeNode node = new TreeNode();
                node.value = i;

                if (i >= 0 && i <= 143) node.codeLength = 8;
                if (i >= 144 && i <= 255) node.codeLength = 9;
                if (i >= 256 && i <= 279) node.codeLength = 7;
                if (i >= 280 && i <= 287) node.codeLength = 8;

                leaves.Add(node);
            }

            int[] nextCode = { 0, 0, 0, 0, 0, 0, 0, 0, 0x30, 0x190};

            for (int i = 0; i < 288; i++)
            {
                if (i == 280)
                    nextCode[8] = 0xC0;

                byte len = leaves[i].codeLength;
                leaves[i].huffmanCode = nextCode[len];
                nextCode[len]++;
            }

            MakeLinks();
        }


        private void MakeLinks()
        {
            // Создаем связи между корнем дерева и листами по кодам Хаффмана

            root = new TreeNode();
            TreeNode curNode = null;

            for (int i = 0; i < leaves.Count; i++)
            {
                curNode = root;

                byte len = leaves[i].codeLength;
                int code = leaves[i].huffmanCode;

                for (int j = len - 1; j >= 0; j--)
                {
                    byte bit = (byte)((code >> j) & 1);

                    if (bit == 0)
                    {
                        if (curNode.left == null)
                        {
                            if (j > 0)
                                curNode.left = new TreeNode();
                            else curNode.left = leaves[i];
                        }
                        curNode = curNode.left;
                    }
                    else if (bit == 1)
                    {
                        if (curNode.right == null)
                        {
                            if (j > 0)
                                curNode.right = new TreeNode();
                            else curNode.right = leaves[i];
                        }
                        curNode = curNode.right;
                    }
                }
            }
        }


        public int ReadCode(BitReader bitr)
        {
            TreeNode curNode = root;

            byte bit = bitr.ReadBit();
            if (bit == 0)
                curNode = curNode.left;
            else if (bit == 1)
                curNode = curNode.right;

            while (!curNode.IsLeaf())
            {
                bit = bitr.ReadBit();
                if (bit == 0)
                    curNode = curNode.left;
                else if (bit == 1)
                    curNode = curNode.right;
            }

            return curNode.value;
        }
    }
}
