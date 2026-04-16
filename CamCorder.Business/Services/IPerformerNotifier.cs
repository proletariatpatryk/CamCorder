using CamCorder.Business.Models;

namespace CamCorder.Business.Services
{
    public interface IPerformerNotifier
    {
        Task PerformerCreatedAsync(PerformerDTO performer);
        Task PerformerUpdatedAsync(PerformerDTO performer);
        Task PerformerDeletedAsync(int performerId);
    }
}
