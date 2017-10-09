using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    /// <summary>
    /// Lzw Node for the node tree. I think this is incredibly memory inefficient.
    /// Need to look at just using some linked lists instead
    /// </summary>
    public class LzwNode
    {
        
        private Dictionary<byte, LzwNode> subNodes = new Dictionary<byte, LzwNode>();

        public LzwNode()
        {

        }

        public LzwNode(long codeValue)
        {
            this.Code = codeValue;
        }
        

        public long Code { get; set; }

        public int Count { get { return subNodes.Count; } }

        public void SetNode(byte b, LzwNode node)
        {
            subNodes.Add(b, node);
        }

        public LzwNode GetNode(byte data)
        {
            LzwNode node;
            subNodes.TryGetValue(data, out node);
            return node;
        }

        public LzwNode GetNode(byte[] data)
        {
            LzwNode current = this;
            for (int i = 0; i < data.Length && current != null; i++)
            {
                current = current.GetNode(data[i]);
            }
            return current;
        }
    }
}
