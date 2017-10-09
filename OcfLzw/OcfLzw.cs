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
    /// This is ported from the LZW filter from the Apache pdfbox project
    /// https://pdfbox.apache.org/.
    /// 
    /// License: http://www.apache.org/licenses/LICENSE-2.0
    /// </summary>
    /// <remarks>
    /// I like this implementation from PDFBox, but it's very heap intensive. 
    /// As I want to run this across millions of rows, the performance just isn't there.
    /// The twelve monkey's LZW Decoder: https://github.com/haraldk/TwelveMonkeys/blob/master/imageio/imageio-tiff/src/main/java/com/twelvemonkeys/imageio/plugins/tiff/LZWDecoder.java
    /// looks like it would be less painful on the heap
    /// </remarks>
    public class OcfLzw
    {
        /// <summary>
        /// The LZW clear table code.
        /// </summary>
        const int CLEAR_TABLE = 256;

        /// <summary>
        /// The LZW end of data code.
        /// </summary>
        const int EOD = 257;

        /// <summary>
        /// The minimum size (in bits) of a compressed chunk
        /// </summary>
        const int MIN_CHUNK_SIZE = 9;


        /// <summary>
        /// The maximum size (in bits) of a compressed chunk
        /// </summary>
        const int MAX_CHUNK_SIZE = 13;

        /// <summary>
        /// The first code in the set
        /// </summary>
        const int FIRST_CODE = 258;

        const int DICTIONARY_SIZE = 9029;

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
            byte firstByte = 0;
            long nextCommand = 0;
            
            input.BitsInChunk = MIN_CHUNK_SIZE;

            var dic = new LzwDictionary(DICTIONARY_SIZE);

            while ((nextCommand = input.Read()) != EOD)
            {
                // Do our reset
                if (nextCommand == CLEAR_TABLE)
                {
                    input.BitsInChunk = MIN_CHUNK_SIZE;
                    dic = new LzwDictionary(DICTIONARY_SIZE);
                }
                else
                {
                    byte[] data = dic.GetData(nextCommand);
                    if (data == null)
                    {
                        dic.Visit(firstByte);
                        data = dic.GetData(nextCommand);
                        dic.Clearbuffer(); // clear buffer
                    }
                    if (data == null)
                    {
                        throw new Exception("Error: data is null");
                    }

                    dic.Visit(data);

                    if (dic.GetNextCode() >= 2047)
                    {
                        input.BitsInChunk = 12;
                    }
                    else if (dic.GetNextCode() >= 1023)
                    {
                        input.BitsInChunk = 11;
                    }
                    else if (dic.GetNextCode() >= 511)
                    {
                        input.BitsInChunk = 10;
                    }
                    else
                    {
                        input.BitsInChunk = 9;
                    }

                    firstByte = data[0];
                    output.Write(data, 0, data.Length);
                }
                
            }
            output.Flush();
            
            
        }

    }
}
