# OcfLzw

Implements "OCF Compresion" as used by Cerner PowerChart. 

The compression is basically just the same LZW compression as used in TIFF images.

Please use the `OCfLzw2` version of this library. The previous version in `OcfLzw` folder is 
no longer under development as it was too slow.

An alternate implementation by Bruce Jackson may also be of interest:
 * https://gist.github.com/pgodwin/7d66729444173146ad698d154f2b9b6c
 * https://blog.brucejackson.info/2013/03/deconstructing-lzw-decompression.html/
 
## Current Status
The code has been extensively tested against more than 10 million "wild" LZW blobs without issue. 
It handles compressed streams with a chunk-size of up to 13 bits. 

## History
I wanted to produce a simple LZW decoder which could specifically handle "BLOB" data from Cerner PowerChart.
I also wanted something which was somewhat "C# like" rather than just another port of the C implementations. 
As such, it trades speed for "style".

This port is based on two Java Implementation of TIFF LZW:

 1. PDFBox was the original source (https://pdfbox.apache.org/.).
 2. TwelveMonkeys TIFF LZWDecoder (https://github.com/haraldk/TwelveMonkeys)

I've made use of PDFBox's NBitStream for reading the variable bit-widths from the stream.
However it's LZW decoder seemed to be very heap-intensive (and I really didn't need the hash overhead of a dictionary),
with decompressing 1 million values taking over 2 min on an i5-4590s.

Instead, the main decoding loop is based on the TwelveMonkeys Image IO extensions,
specfically - https://github.com/haraldk/TwelveMonkeys/blob/master/imageio/imageio-tiff/src/main/java/com/twelvemonkeys/imageio/plugins/tiff/LZWDecoder.java

Rather than a dictionary and node-tree, it just uses an array. This reduces the number of objects created, and has a much
more predicatable memory footprint.

As a result, running 1 million values on the same i5-4590s now completes within ~30 seconds.

I think there's room for further optimisation, but I want to try and ensure the code remains readable.


### Todo:

 * [ ] Package and publish as nuget packages
 * [ ] Add SQL CLR functions (likely as a separate library)
 * [ ] Further performance improvements

## Licenses
License: http://www.apache.org/licenses/LICENSE-2.0

Note that TwelveMonkeys is released under 3-Clause BSD which should be compatible with Apache. 
If you're unsure or uncomfortable, please do not use this code.
