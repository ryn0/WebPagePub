using System;

namespace WebPagePub.Services.Helpers
{


    public static class MemoryCacheSizeEstimator
    {
        public static long EstimateKb(params string?[] values)
        {
            long bytes = 64; // small overhead
            foreach (var v in values)
            {
                if (!string.IsNullOrEmpty(v))
                {
                    bytes += (long)v.Length * 2; // UTF-16 ~2 bytes/char
                }
            }
            long kb = Math.Max(1, (bytes + 1023) / 1024);
            return kb;
        }
    }

}
