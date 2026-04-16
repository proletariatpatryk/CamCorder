using System;
using System.Collections.Generic;
using System.Text;

namespace CamCorder.Business.Services
{
    public interface ICamSite
    {
        Task<PageInfo> GetPageInfoAsync(int performerId, CancellationToken cancellationToken = default);
    }
}
