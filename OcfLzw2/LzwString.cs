using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcfLzw
{
    public class LZWString : IComparable<LZWString>
    {
        /// <summary>
        /// Empty version of this string for comparisons (though can probably use null?)
        /// </summary>
        internal static LZWString EMPTY = new LZWString((byte) 0, (byte) 0, 0, null);

        private LZWString previous;

        public int length;
        public byte value;
        public byte firstChar; // Copied forward for fast access

        private byte[] writeBuffer; // Buffer used for writing out to the stream later

        public LZWString(byte code) : this(code, code, 1, null) { }

        public LZWString(byte value, byte firstChar, int length, LZWString previous) {
            this.value = value;
            this.firstChar = firstChar;
            this.length = length;
            writeBuffer = new byte[length];
            this.previous = previous;
        }

        public LZWString concatenate(byte value)
        {
            if (this == EMPTY)
            {
                return new LZWString(value);
            }

            return new LZWString(value, this.firstChar, length + 1, this);
        }

        public void writeTo(Stream buffer)
        {
            if (length == 0)
            {
                return;
            }

            if (length == 1)
            {
                buffer.WriteByte(value);
            }
            else
            {
                LZWString e = this;
                long offset = buffer.Position;

                // We're using a stream.
                // We can't assume we're going to be able to seek it in every situation
                // (ie we might be writing it out to a HTTP response, we can't very well go "oh wait here's some more data back here")
                // So store the values in our small buffer and write it out to the stream.
                for (int i = length - 1; i >= 0; i--)
                {
                    writeBuffer[i] = e.value;
                    //buffer.put(offset + i, e.value);
                    //buffer.WriteByte(e.value);
                    e = e.previous;
                }

                //buffer.position(offset + length);
                buffer.Write(writeBuffer, 0, length);
            }
        }

        #region Overrides
        public override int GetHashCode()
        {
            int result = previous != null ? previous.GetHashCode() : 0;
            result = 31 * result + length;
            result = 31 * result + (int)value;
            result = 31 * result + (int)firstChar;
            return result;
        }

        /// <summary>
        /// Checks to see if the two objects are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null || other.GetType() != this.GetType())
            {
                return false;
            }

            LZWString s = (LZWString)other;

            return firstChar == s.firstChar &&
                    length == s.length &&
                    value == s.value &&
                    previous == s.previous;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("ZLWString[");
            int offset = builder.Length;
            LZWString e = this;
            for (int i = length - 1; i >= 0; i--)
            {
                builder.Insert(offset, String.Format("%2x", e.value));
                e = e.previous;
            }
            builder.Append("]");
            return builder.ToString();
        }

        public int CompareTo(LZWString other)
        {
            if (other == this)
            {
                return 0;
            }

            if (length != other.length)
            {
                return other.length - length;
            }

            if (firstChar != other.firstChar)
            {
                return other.firstChar - firstChar;
            }

            LZWString t = this;
            LZWString o = other;

            for (int i = length - 1; i > 0; i--)
            {
                if (t.value != o.value)
                {
                    return o.value - t.value;
                }

                t = t.previous;
                o = o.previous;
            }

            return 0;
        }

        #endregion
    }
}
