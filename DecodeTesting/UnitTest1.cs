using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace DecodeTesting
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DecodeTest()
        {
            var base64Value = @"PZcOR0MwxLhhNxzNMIhRpMZwM4xGQ1GRcMhlMxmGEXjJshMSGAzGcCMxvNx0OhiNkljhmNxpNhcMxjNBhORzMp0GAgKhhNBvNphHZ9PoNBRcOxpMp3NZpNxkGhcOpjg5wm5kmcuOYynhlORyN5yLlXOVHowKgIAAAG9jZl9ibG9iAA==";

            var bytes = Convert.FromBase64String(base64Value);
            var compressedString = Encoding.ASCII.GetString(bytes);

            var resultString = 
@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Tahoma;}}
\viewkind4\uc1\pard\f0\fs20 error\par
}
";
            var decoded = Encoding.Default.GetString(OcfLzw.OcfLzw.Decode(bytes));
            
            Assert.AreEqual(resultString, decoded);
        }

        [TestMethod]
        public void DecodeTest2()
        {
            var base64Value = @"PZcOR0MwxLhhNxzNMIhRpMZwM4xGQ1GRcMhlMxmGEXjJshMSGAzGcCMxvNx0OhiNkljhmNxpNhcMxjNBhORzMp0GAgKhhNBvNphHZ9PoNBRcOxpMp3NZpNxkGhcOpjg5wm5kmcuOYynhlORyN5yLlXOVHowKgIAAAG9jZl9ibG9iAA==";

            var bytes = Convert.FromBase64String(base64Value);
            var compressedString = Encoding.ASCII.GetString(bytes);

            var resultString =
@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Tahoma;}}
\viewkind4\uc1\pard\f0\fs20 error\par
}
";
            var decoded = Encoding.Default.GetString(OcfLzw.OcfLzw2.Decode(bytes));

            Assert.AreEqual(resultString, decoded);
        }

        [TestMethod]
        public void Benchmark()
        {
            // Runs the decode method 1,000,000 to check the performance.
            int count = 1000000;
            var base64Value = @"PZcOR0MwxLhhNxzNMIhRpMZwM4xGQ1GRcMhlMxmGEXjJshMSGAzGcCMxvNx0OhiNkljhmNxpNhcMxjNBhORzMp0GAgKhhNBvNphHZ9PoNBRcOxpMp3NZpNxkGhcOpjg5wm5kmcuOYynhlORyN5yLlXOVHowKgIAAAG9jZl9ibG9iAA==";

            var bytes = Convert.FromBase64String(base64Value);
            for (int i = 0; i < count; i++)
            {
                OcfLzw.OcfLzw.Decode(bytes);
            }
        }

        [TestMethod]
        public void Benchmark2()
        {
            // Runs the decode method 1,000,000 to check the performance.
            int count = 1000000;
            var base64Value = @"PZcOR0MwxLhhNxzNMIhRpMZwM4xGQ1GRcMhlMxmGEXjJshMSGAzGcCMxvNx0OhiNkljhmNxpNhcMxjNBhORzMp0GAgKhhNBvNphHZ9PoNBRcOxpMp3NZpNxkGhcOpjg5wm5kmcuOYynhlORyN5yLlXOVHowKgIAAAG9jZl9ibG9iAA==";

            var bytes = Convert.FromBase64String(base64Value);
            for (int i = 0; i < count; i++)
            {
                OcfLzw.OcfLzw2.Decode(bytes);
            }
        }
    }
}
