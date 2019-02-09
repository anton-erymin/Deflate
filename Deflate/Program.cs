using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Deflate
{
    class Program
    {

        static void Main(string[] args)
        {
            ZLIBDecoder zlibDecoder = new ZLIBDecoder();
            zlibDecoder.Decode("c.txt");

            //Console.ReadKey();
        }


    }
}