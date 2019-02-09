using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace Deflate
{
    static class Log
    {
        private static String path;
        private static StreamWriter writer;
        private static int depth;



        static Log()
        {
            path = @"Log.txt";
        }

        
        public static void WriteTree(HuffmanTree tree)
        {
            writer = new StreamWriter(File.Create(path));

            depth = -1;
            TreeRecursive(tree.root);
            
            writer.Close();
        }


        private static void TreeRecursive(TreeNode node)
        {
            depth++;

            StringBuilder line = new StringBuilder();
            line.Append('\t', depth);

            if (!node.IsLeaf())
            {
                line.Append('*');
            }
            else
            {
                line.Append(node.value.ToString() + "\t");

                for (int i = node.codeLength - 1; i >= 0; i--)
                {
                    line.Append((node.huffmanCode >> i) & 1);
                }
            }

            writer.WriteLine(line);

            
            if (node.left != null)
            {
                TreeRecursive(node.left);
            }

            if (node.right != null)
            {
                TreeRecursive(node.right);
            }

            depth--;
        }
    }
}
