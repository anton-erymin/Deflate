using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deflate
{
    class ZLIBDecoder
    {
        private const int DEFLATE = 8;


        public void Decode(String fname)
        {
            FileStream ifs = File.OpenRead(fname);
            BinaryReader ibr = new BinaryReader(ifs);

            byte cmf = ibr.ReadByte();
            byte method = (byte)(cmf & 0xF);

            if (method != DEFLATE)
                return;

            int bufferSize = (byte)((cmf >> 4) & 0xF);
            bufferSize = 1 << (bufferSize + 8);

            byte flag = ibr.ReadByte();
            byte fdict = (byte)((flag >> 5) & 1);

            if (fdict != 0)
                return;

            DeflateDecoder deflate = new DeflateDecoder(bufferSize);
            deflate.Decode(ibr);



            ibr.Close();
            ifs.Close();
        }
    }
}
