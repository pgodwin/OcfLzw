using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    public class LzwDictionary : Dictionary<long, byte[]>
    {
        private long nextCode = 258; // Next code value
        private int codeSize = 9; // Default code size ...

        // Set our buffer size to prevent having to reallocate the memory over and over
        private MemoryStream buffer = new MemoryStream(9029);

        /// <summary>
        /// This is the used for the LZWDecode filter.  This represents the dictionary mappings
        /// between codes and their values.
        /// </summary>
        /// <remarks>
        public LzwDictionary(int size) : base(size)
        {
            Root = new LzwNode();
            
            for (long i = 0; i < 256; i++)
            {
                LzwNode node = new LzwNode();
                node.Code = i;
                this.Root.SetNode((byte)i, node);
                this.Add(i, new byte[] { (byte)i });
            }
        }

        public LzwNode Root { get; set; }

        /// <summary>
        /// This will get the value for the code. It will return null if the code is not
        /// defined.
        /// </summary>
        /// <param name="code">The key to the data.</param>
        /// <returns>The data that is mapped to the code.</returns>
        public byte[] GetData(long code)
        {
            byte[] data;
            this.TryGetValue(code, out data);
            return data;
        }

        /// <summary>
        /// This will take a visit from a byte[].  This will create new code entries as
        /// necessary.
        /// </summary>
        /// <param name="data">The bytes to get a visit from.</param>
        public void Visit(byte[] data) 
        {
            for( int i = 0; i<data.Length; i++ )
            {
                this.Visit(data[i] );
            }
        }

        /// <summary>
        /// This will take a visit from a byte.  This will create new code entries as
        /// necessary.
        /// </summary>
        /// <param name="data">The byte to get a visit from.</param>
        public void Visit(byte data)
        {
            buffer.WriteByte(data);
            byte[] curBuffer = buffer.ToArray();

            LzwNode previous = null;
            LzwNode current = this.Root;

            bool createNewCode = false;

            for (int i = 0; i < curBuffer.Length && current != null; i++)
            {
                previous = current;
                current = current.GetNode(curBuffer[i]);
                if (current == null)
                {
                    createNewCode = true;
                    current = new LzwNode();
                    previous.SetNode(curBuffer[i], current);
                }
            }   

            if (createNewCode)
            {
                long code = nextCode++;
                current.Code = code;
                this.Add(code, curBuffer);

                buffer = new MemoryStream();
                buffer.WriteByte(data);
                resetCodeSize();

            }
        }

       
        
       /// <summary>
       /// This will determine the code size.
       /// </summary>
        private void resetCodeSize()
        {
            if (nextCode >= 2048)
            {
                codeSize = 12;
            }
            else if (nextCode >= 1024)
            {
                codeSize = 11;
            }
            else if (nextCode >= 512)
            {
                codeSize = 10;
            }
            else
            {
                codeSize = 9;
            }
        }

        /// <summary>
        /// This will crear the internal buffer that the dictionary uses.
        /// </summary>
        public void Clearbuffer()
        {
            buffer = new MemoryStream();
        }

        /// <summary>
        /// This will get the next code that will be created.
        /// </summary>
        /// <returns></returns>
        public long GetNextCode()
        {
            return nextCode;
        }
    }


}
