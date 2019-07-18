using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    /// <summary>
    /// "OCF Compresion" is used by Cerner PowerChart. 
    /// It's basically just the LZW compression used in TIFF images.
    /// License: http://www.apache.org/licenses/LICENSE-2.0
    /// </summary>
    /// <remarks>
    /// This port is based on two Java Implementation of TIFF LZE. 
    /// PDFBox was the original source (https://pdfbox.apache.org/.).
    /// I've made use of it's NBitStream for reading the variable bit-widths from the stream.
    /// However PDFBox was very heap-intensive (and really didn't need the hash of a dictionary),
    /// with decompressing 1 million values taking over 1 min on an i5-4590s.
    /// As I want to run this across millions of rows, the performance just isn't there.
    /// Instead the main decoding is from the TwelveMonkeys Image IO extensions,
    /// specfically - https://github.com/haraldk/TwelveMonkeys/blob/master/imageio/imageio-tiff/src/main/java/com/twelvemonkeys/imageio/plugins/tiff/LZWDecoder.java
    /// Rather than a dictionary and node-tree, it uses a much more sensible array of LzwString, which have 
    /// (with the exception of the buffer) a static memory footprint. This reduces the number of objects created.
    /// As a result, running 1 million values on the same i5-4590s now completes within ~30 seconds.
    /// I think there's room for further optimisation, but I want to try and ensure the code remains readable.
    /// </remarks>
    public class OcfLzw2
    {
        /// <summary>
        /// The LZW clear table code.
        /// </summary>
        internal const int CLEAR_TABLE = 256;

        /// <summary>
        /// The LZW end of data code.
        /// </summary>
        internal const int EOD = 257;

        /// <summary>
        /// The minimum size (in bits) of a compressed chunk
        /// </summary>
        internal const int MIN_CHUNK_SIZE = 9;


        /// <summary>
        /// The maximum size (in bits) of a compressed chunk
        /// </summary>
        internal const int MAX_CHUNK_SIZE = 13;

        /// <summary>
        /// The first code in the set
        /// </summary>
        internal const int FIRST_CODE = 258;

        //internal const int DICTIONARY_SIZE = 9029;

        internal const int CODE_SIZE = 9; // Default code size ...

        /// <summary>
        /// Expected table size
        /// </summary>
        internal const int TABLE_SIZE = 1 << MAX_CHUNK_SIZE;

        /// <summary>
        /// Decodes the specified compressed string (ideally 8 bit chars) and returns the decompressed string
        /// </summary>
        /// <param name="compressed">LZW encoded string</param>
        /// <returns>Decoded string</returns>
        public static string Decode(string compressed)
        {
            return Encoding.Default.GetString(Decode(Encoding.Default.GetBytes(compressed)));
        }

        /// <summary>
        /// Decodes the specified byte array
        /// </summary>
        /// <param name="compressed">Compressed Bytes</param>
        /// <returns>byte array of decompressed data</returns>
        public static byte[] Decode(byte[] compressed)
        {
            using (var reader = new MemoryStream(compressed))
            using (var output = new MemoryStream())
            {
                Decode(reader, output);
                return output.ToArray();
            }   
        }

        /// <summary>
        /// Decodes the data in the stream with the LZW decoder
        /// </summary>
        /// <param name="compressedInput">Stream of compressed data</param>
        /// <param name="output">Stream of decompressed data</param>
        public static void Decode(Stream compressedInput, Stream output)
        {
            NBitStream input = new NBitStream(compressedInput);
            //int bitsInChunk = MIN_CHUNK_SIZE; // Min size
            //byte firstByte = 0;
            int nextCommand = 0;

            int lastCommand = CLEAR_TABLE;


            LzwTable table = new LzwTable(TABLE_SIZE);
            
            // The input needs the bits in chunk set properly to read the data
            input.BitsInChunk = MIN_CHUNK_SIZE;

            while ((nextCommand = (int)input.Read()) != EOD)
            {
                if (nextCommand < 0)
                {
                    break;
                }

                // Do our reset
                if (nextCommand == CLEAR_TABLE)
                {
                    input.BitsInChunk = MIN_CHUNK_SIZE;
                    nextCommand = (int)input.Read();
                    
                    if (nextCommand == EOD)
                    {
                        break;
                    }
                    
                    if (table[nextCommand] == null)
                    {
                        throw new Exception(String.Format("Corrupted LZW: code {0} (table size: {1})", nextCommand, table.Length));
                    }

                    // Write out anything that's left in our table to the output stream
                    table[nextCommand].writeTo(output);
                    // Do I need to flush out the table...i think yes as we're using list.add() rather than array references?
                    // We could save some cycles not rebuilding the first 256 ASCII codes
                    // But I don't see the point.
                    table = new LzwTable(TABLE_SIZE); 
                }
                else
                {
                    //if (table[lastCommand] == null)
                    //{
                    //    throw new Exception(String.Format("Corrupted LZW: code {0} (table size: {1})", lastCommand, table.Length));
                    //}

                    // Check if the command is already in the table
                    // We could use a dictionary here, but I don't think we need the extra overhead of a hash
                    if (nextCommand < table.Length)
                    {
                        table[nextCommand].writeTo(output);

                        table.Add(table[lastCommand].concatenate(table[nextCommand].firstChar));
                    }
                    else
                    {
                        LZWString outString = table[lastCommand].concatenate(table[lastCommand].firstChar);

                        outString.writeTo(output);
                        table.Add(outString);
                    }

                    // The input needs the bits in chunk set properly to read the data
                    if (table.GetNextCode() >= 2047)
                    {
                        input.BitsInChunk = 12;
                    }
                    else if (table.GetNextCode() >= 1023)
                    {
                        input.BitsInChunk = 11;
                    }
                    else if (table.GetNextCode() >= 511)
                    {
                        input.BitsInChunk = 10;
                    }
                    else
                    {
                        input.BitsInChunk = 9;
                    }

                    lastCommand = nextCommand;
                }
                
            }

            output.Flush();
        }

    }
}
