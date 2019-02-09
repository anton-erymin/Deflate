using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deflate
{
    class BitReader
    {
        private BinaryReader br;
        private int buffer;
        private int bitPos;


        public BitReader(BinaryReader br)
        {
            this.br = br;
            bitPos = 32;
        }


        public int ReadBits(int numBits)
        {
            if (bitPos > 31)
                GetNext();

            int res = 0;

            if (numBits <= 32 - bitPos)
            {
                res = (buffer >> bitPos) & (int)(0xFFFFFFFF >> (32 - numBits));
                bitPos += numBits;
                
            }
            else
            {
                int resBits = 32 - bitPos;
                int a = (buffer >> bitPos) & (int)(0xFFFFFFFF >> (32 - resBits));
                GetNext();
                int moreBits = numBits - resBits;
                int b = buffer & (int)(0xFFFFFFFF >> (32 - moreBits));
                res = a | (b << resBits);

                bitPos += moreBits;
            }

            return res;
        }


        public byte ReadBit()
        {
            return (byte)ReadBits(1);
        }


        private void GetNext()
        {
            buffer = br.ReadInt32();
            bitPos = 0;
        }
    }
}
