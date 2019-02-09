using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deflate
{
    class DeflateDecoder
    {
        private const int TYPE_NO_COMPRESSION  = 0;
        private const int TYPE_FIXED_HUFFMAN   = 1;
        private const int TYPE_DYNAMIC_HUFFMAN = 2;
        private const int TYPE_RESERVED        = 3;

        // Порядок символов в алфавите длин кодов
        private byte[] cwl = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        private byte[] extraLengthBits = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };
        private byte[] extraDistBits = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

        private int[] extraLengthValues = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258 };
        private int[] extraDistValues = { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 
                                            6145, 8193, 12289, 16385, 24577 };


        private BitReader bitr;

        private int bufferSize;
        DictionaryBuffer dict;

        private HuffmanTree cwlTree;

        private StreamWriter writer;

        public DeflateDecoder(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }


        public void Decode(BinaryReader br)
        {
            writer = new StreamWriter(File.Create("out.txt"));
            
            bitr = new BitReader(br);

            dict = new DictionaryBuffer(bufferSize);

            byte lastBlock = 0;

            do
            {
                lastBlock = bitr.ReadBit();
                DecodeBlock();
                //lastBlock = 1;

            } while (lastBlock == 0);

            writer.Close();
        }


        private void DecodeBlock()
        {
            int blockType = bitr.ReadBits(2);

            switch (blockType)
            {
                case TYPE_DYNAMIC_HUFFMAN:
                    {
                        DynamicHuffman();
                        break;
                    }
                case TYPE_FIXED_HUFFMAN:
                    {
                        FixedHuffman();
                        break;
                    }
            }
        }


        private void FixedHuffman()
        {
            HuffmanTree basicTree = new HuffmanTree();
            basicTree.BuildFixedHuffmanTree();
            // Логируем результат создания дерева
            Log.WriteTree(basicTree);

            // Декодируем сами сжатые данные

            int code;
            do
            {
                code = basicTree.ReadCode(bitr);

                if (code >= 0 & code <= 255)
                {
                    char c = (char)code;
                    writer.Write(c);
                    dict.Add(c);
                }
                else if (code > 256)
                {
                    byte numExtraBits = extraLengthBits[code - 257];
                    int extraBits = 0;
                    if (numExtraBits != 0)
                        extraBits = bitr.ReadBits(numExtraBits);

                    int len = extraLengthValues[code - 257] + extraBits;
                    extraBits = 0;

                    int codeDist = bitr.ReadBits(5);

                    numExtraBits = extraDistBits[codeDist];
                    if (numExtraBits != 0)
                        extraBits = bitr.ReadBits(numExtraBits);

                    int dist = extraDistValues[codeDist] + extraBits;

                    String s = dict.GetFrase(len, dist);
                    writer.Write(s);
                    dict.Add(s);
                }

            } while (code != 256);
        }


        private void DynamicHuffman()
        {
            byte hlit =(byte)bitr.ReadBits(5);
            byte hdist = (byte)bitr.ReadBits(5);
            byte hclen = (byte)bitr.ReadBits(4);

            byte[] codeLengthsForCWL = new byte[cwl.Length];
            for (int i = 0; i < cwl.Length; i++)
                codeLengthsForCWL[i] = 0;

            // Считываем длины кодов алфавита длин кодов
            for (int i = 0; i < hclen + 4; i++)
            {
                byte cl = (byte)bitr.ReadBits(3);
                codeLengthsForCWL[cwl[i]] = cl;
            }

            // Строим дерево алфавита длин кодов
            cwlTree = new HuffmanTree();
            cwlTree.BuildHuffmanTree(codeLengthsForCWL);

            // Читаем с использованием построенного дерева длины кодов алфавита символов и длин и строим дерево
            HuffmanTree basicTree = ReadTree(hlit + 257);
            HuffmanTree distTree = ReadTree(hdist + 1);


            // Декодируем сами сжатые данные
            int code;
            do
            {
                code = basicTree.ReadCode(bitr);

                if (code >= 0 & code <= 255)
                {
                    char c = (char)code;
                    writer.Write(c);
                    dict.Add(c);
                }
                else if (code > 256)
                {
                    byte numExtraBits = extraLengthBits[code - 257];
                    int extraBits = 0;
                    if (numExtraBits != 0)
                        extraBits = bitr.ReadBits(numExtraBits);

                    int len = extraLengthValues[code - 257] + extraBits;
                    extraBits = 0;

                    int codeDist = distTree.ReadCode(bitr);

                    numExtraBits = extraDistBits[codeDist];
                    if (numExtraBits != 0)
                        extraBits = bitr.ReadBits(numExtraBits);

                    int dist = extraDistValues[codeDist] + extraBits;

                    String s = dict.GetFrase(len, dist);
                    writer.Write(s);
                    dict.Add(s);
                }

            } while(code != 256);
        }


        private HuffmanTree ReadTree(int num)
        {
            byte[] codeLengths = new byte[num];
            int k = 0;
            while (k < codeLengths.Length)
            {
                int code = cwlTree.ReadCode(bitr);

                if (code >= 0 && code <= 15)
                {
                    codeLengths[k++] = (byte)code;
                }
                else
                {
                    switch (code)
                    {
                        case 16:
                            {
                                int numRepeats = bitr.ReadBits(2) + 3;
                                int prevCodeLength = codeLengths[k - 1];
                                for (int j = 1; j <= numRepeats; j++, k++)
                                    codeLengths[k] = (byte)prevCodeLength;
                                break;
                            }
                        case 17:
                            {
                                int numRepeats = bitr.ReadBits(3) + 3;
                                for (int j = 1; j <= numRepeats; j++, k++)
                                    codeLengths[k] = 0;
                                break;
                            }
                        case 18:
                            {
                                int numRepeats = bitr.ReadBits(7) + 11;
                                for (int j = 1; j <= numRepeats; j++, k++)
                                    codeLengths[k] = 0;
                                break;
                            }
                    }
                }
            }

            HuffmanTree tree = new HuffmanTree();
            tree.BuildHuffmanTree(codeLengths);

            // Логируем результат создания дерева
            Log.WriteTree(tree);

            return tree;
        }
    }
}