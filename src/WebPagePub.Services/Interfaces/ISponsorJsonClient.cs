using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebPagePub.Services.Models.Sponsors;


namespace WebPagePub.Services.Interfaces
{
    public interface ISponsorJsonClient
    {
        Task<IReadOnlyList<SponsorCardItem>> GetActiveSponsorsAsync(CancellationToken ct = default);

        Task<IReadOnlyList<SponsorCardItem>> GetMainSponsorsAsync(CancellationToken ct = default);
    }
}
