using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenchmarkProfiling
{
    class Program
    {
        static void Main(string[] args)
        {
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
