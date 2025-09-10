
// WebPagePub.Data.Models.Transfer/PagedResult.cs
using System;
using System.Collections.Generic;

namespace WebPagePub.Data.Models.Transfer
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }
}
