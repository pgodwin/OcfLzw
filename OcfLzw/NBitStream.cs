using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    /// <summary>
    /// This is an n-bit input stream.  This means that you can read chunks of data
    /// as any number of bits, not just 8 bits like the regular input stream.Just set the
    /// number of bits that you would like to read on each call.  The default is 8.
    /// </summary>
    public class NBitStream 
    {
        private Stream inputStream;
        private int bitsLeftInCurrentByte;
        private int currentByte;

        /// <summary>
        /// Initialise a new input stream
        /// </summary>
        /// <param name="inputStream">The input stream to readfrom</param>
        public NBitStream(Stream inputStream)
        {
            this.inputStream = inputStream;
            this.BitsInChunk = 8; // default 8 bits in the chunk
        }

        /// <summary>
        /// The number of Bits To Read on next Read(). Defaults to 8.s
        /// </summary>
        public int BitsInChunk { get; set; }

        /// <summary>
        /// Original Stream wrapped by this stream.
        /// </summary>
        public Stream Stream { get { return inputStream; } }

        /// <summary>
        /// This will read the next n bits from the stream and return the unsigned
        /// value of  those bits.
        /// </summary>
        /// <returns>The next n bits from the stream.</returns>
        public long Read()
        {
            long retval = 0;
            for (int i = 0; i < BitsInChunk && retval != -1; i++)
            {
                if (bitsLeftInCurrentByte == 0)
                {
                    currentByte = inputStream.ReadByte();
                    bitsLeftInCurrentByte = 8;
                }
                if (currentByte == -1)
                {
                    retval = -1;
                }
                else
                {
                    retval <<= 1;
                    retval |= ((currentByte >> (bitsLeftInCurrentByte - 1)) & 0x1);
                    bitsLeftInCurrentByte--;
                }
            }
            return retval;
        }
    }
}
