using CamCorder.Business.Models;
using CamCorder.Business.Services;
using Microsoft.AspNetCore.SignalR;

namespace CamCorder.WebApp.Hubs
{
    public class PerformerHub : Hub<IPerformerNotifier>
    {
        public async Task SendPerformerUpdated(PerformerDTO performer)
        {
            // Broadcast to all connected clients that a performer was updated
            await Clients.All.PerformerUpdatedAsync(performer);
        }
    }
}
