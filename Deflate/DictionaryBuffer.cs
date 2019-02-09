using System;
using System.Collections.Generic;
using System.Text;



namespace Deflate
{
    class DictionaryBuffer
    {
        private byte[] buffer;
        private int size;
        private short start;
        private short end;
        private bool scroll;
        private int numUsages = 0;

        public DictionaryBuffer(int size)
        {
            this.size = size;
            buffer = new byte[2 * size];
            start = 0;
            end = -1;
            scroll = false;
        }


        public void Add(char c)
        {
            if (!scroll)
            {
                buffer[++end] = (byte)c;
                if (end == size - 1)
                {
                    scroll = true;
                }
            }
            else
            {
                start++;
                buffer[++end] = (byte)c;
                if (end == 2 * size - 1)
                {
                    Move();
                }
            }
        }


        public void Add(String s)
        {
            if (!scroll)
            {
                short restBytes = (short)(size - end - 1);

                if (s.Length <= restBytes)
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        buffer[++end] = (byte)s[i];
                    }
                }
                else
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        buffer[++end] = (byte)s[i];
                        if (end >= size)
                        {
                            start++;
                        }
                    }
                    scroll = true;
                }
            }
            else
            {
                short restBytes = (short)(2 * size - end - 1);
                if (s.Length <= restBytes)
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        buffer[++end] = (byte)s[i];
                        start++;
                    }
                }

                if (end == 2 * size - 1)
                {
                    Move();
                }
            }
        }


        private void Move()
        {
            for (int i = 0, j = size; i < size; i++, j++)
            {
                buffer[i] = buffer[j];
            }
        }


        public String GetFrase(int len, int dist)
        {
            StringBuilder buf = new StringBuilder();
            for (int k = 0; k <= end; k++)
                buf.Append((char)buffer[k]);
            String sbuf = buf.ToString();

            StringBuilder s = new StringBuilder();
            sbuf.Clone();
            numUsages++;
            int i = end - dist + 1;
            if (i < 0) 
                return "";
            for (int j = 0; j < len; j++, i++)
            {
                if (i > end)
                    i = end - dist + 1;

                s.Append((char)buffer[i]);
            }

            return s.ToString();
        }

    }
}
