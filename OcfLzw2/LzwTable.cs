using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    public class LzwTable : List<LZWString>
    {
        public LzwTable(int capacity) : base(capacity) // Set the initial capacity to reduce memory copies
        {
            
            for (int i = 0; i < 256; i++)
            {
                base.Add(new LZWString((byte)i));
            }
            base.Add(null); // 256 EOD
            base.Add(null); // 257 CLEAR_TABLE
        }

               
        public bool IsInTable(int code)
        {
            return code < GetNextCode();
        }

        public new void Add(LZWString lzwString)
        {
            // This really can never happen as we're using a List
            if (this.GetNextCode() > this.Capacity)
                 throw new Exception($"LZW with more than {OcfLzw2.MAX_CHUNK_SIZE} bits per code encountered (table overflow)");
            
            base.Add(lzwString);

            // Determins the maximum code for the given bit value (ie 511 at 9 bits through to 4096 at 12 bits)
            //var maximumCode = (1 << codeSize) - 1;
                        
        }

        public int GetNextCode()
        {
            //return nextCode;
            return this.Count;
        }
        
        /// <summary>
        /// Provide some array compatiblity
        /// </summary>
        public int Length {  get { return this.Count; } }


    }
}
