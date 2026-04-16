using CamCorder.Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CamCorder.Business.Services
{
    public interface IPerformerService
    {
        Task<IEnumerable<PerformerDTO>> GetPerformersAsync();
        Task<PerformerDTO?> GetPerformerByIdAsync(int id);
        Task<PerformerDTO> CreatePerformerAsync(PerformerDTO performer);
        Task<bool> UpdatePerformerAsync(PerformerDTO performer);
        Task<bool> DeletePerformerAsync(int id);
    }
}
