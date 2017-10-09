using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    public class LzwTable : List<LZWString>
    {

        private int nextCode = 258; // our 256 ascii entries + 2 other bytes
        private int codeSize = 9; // Default code size ...

        public LzwTable(int capacity) : base(capacity)
        {
            for (int i = 0; i < 256; i++)
            {
                base.Add(new LZWString((byte)i));
            }
            base.Add(LZWString.EMPTY);
        }

               
        public bool IsInTable(int code)
        {
            return code < nextCode;
        }

        public new void Add(LZWString lzwString)
        {
            // Determins the maximum code for the given bit value (ie 511 at 9 bits through to 4096 at 12 bits)
            var maximumCode = (1 << codeSize) - 1;

            if (this.nextCode > maximumCode)
            {
                throw new Exception($"LZW with more than {OcfLzw2.MAX_CHUNK_SIZE} bits per code encountered (table overflow)");
            }

            base.Add(lzwString);
            nextCode++;

            resetCodeSize();
        }

        public int GetNextCode()
        {
            return nextCode;
        }

        /// <summary>
        /// This will determine the code size.
        /// </summary>
        private void resetCodeSize()
        {
            if (nextCode >= 4096)
            {
                codeSize = 13;
            }
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
        /// Provide some array compatiblity
        /// </summary>
        public int Length {  get { return this.Count; } }


    }
}
