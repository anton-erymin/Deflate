using System;
using System.Collections.Generic;
using System.Text;

namespace Deflate
{
    class TreeNode
    {
        public TreeNode left = null;
        public TreeNode right = null;

        public int value = 0;
        public int huffmanCode = 0;
        public byte codeLength = 0;


        public bool IsLeaf()
        {
            if (left == null && right == null)
                return true;
            else return false;
        }
    }
}