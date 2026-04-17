using CamCorder.Business.Models;
using CamCorder.Business.Services;
using CamCorder.Common;
using CamCorder.WebApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CamCorder.WebApp.Services
{
    [Injectable(typeof(IPerformerNotifier), ServiceLifetime.Singleton)]
    public class PerformerNotifier(IHubContext<PerformerHub> hubContext) : IPerformerNotifier
    {
        private readonly IHubContext<PerformerHub> _hubContext = hubContext;

        public async Task PerformerUpdatedAsync(PerformerDTO performer)
        {
            await _hubContext.Clients.All.SendAsync("PerformerUpdated", performer);
        }
        public async Task PerformerCreatedAsync(PerformerDTO performer)
        {
            await _hubContext.Clients.All.SendAsync("PerformerCreated", performer);
        }

        public async Task PerformerDeletedAsync(int performerId)
        {
            await _hubContext.Clients.All.SendAsync("PerformerDeleted", performerId);
        }
    }
}
