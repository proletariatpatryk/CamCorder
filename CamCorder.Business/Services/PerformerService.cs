using CamCorder.Business.Models;
using CamCorder.Common;
using CamCorder.Data;
using CamCorder.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CamCorder.Business.Services
{
    [Injectable(typeof(IPerformerService), ServiceLifetime.Scoped)]
    public class PerformerService(CamCorderContext context, ILogger<IPerformerService> logger, IPerformerNotifier? notifier = null) : IPerformerService
    {
        private readonly ILogger<IPerformerService> _logger = logger;
        private readonly CamCorderContext _context = context;
        private readonly IPerformerNotifier? _notifier = notifier;

        public async Task<IEnumerable<PerformerDTO>> GetPerformersAsync()
        {
            var performers = await _context.Performers.AsNoTracking().ToListAsync();
            return performers.Select(ToDto);
        }

        public async Task<PerformerDTO?> GetPerformerByIdAsync(int id)
        {
            var performer = await _context.Performers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (performer == null) 
                return null;

            return ToDto(performer);
        }

        public async Task<PerformerDTO> CreatePerformerAsync(PerformerDTO performer)
        {
            var entity = new Performer
            {
                Name = performer.Name,
                Url = performer.Url
            };

            await _context.Performers.AddAsync(entity);
            await _context.SaveChangesAsync();

            performer.Id = entity.Id;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created performer {Id} - {Name}", entity.Id, entity.Name);

            try
            {
                if (_notifier != null)
                {
                    await _notifier.PerformerCreatedAsync(performer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify about performer creation {Id}", performer.Id);
            }
            return performer;
        }

        public async Task<bool> UpdatePerformerAsync(PerformerDTO performer)
        {
            var entity = await _context.Performers.FirstOrDefaultAsync(p => p.Id == performer.Id);
            if (entity == null)
            {
                _logger.LogWarning("Attempted to update non-existing performer {Id}", performer.Id);
                return false;
            }

            entity.Name = performer.Name;
            entity.Url = performer.Url;

            _context.Performers.Update(entity);
            await _context.SaveChangesAsync();

            if(_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Updated performer {Id}", performer.Id);
            
            
            // Notify interested parties that a performer was updated
            try
            {
                if (_notifier != null)
                {
                    await _notifier.PerformerUpdatedAsync(ToDto(entity));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify about performer update {Id}", performer.Id);
            }
            return true;
        }

        public async Task<bool> DeletePerformerAsync(int id)
        {
            var entity = await _context.Performers.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null)
            {
                _logger.LogWarning("Attempted to delete non-existing performer {Id}", id);
                return false;
            }

            _context.Performers.Remove(entity);
            await _context.SaveChangesAsync();

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Deleted performer {Id}", id);

            try
            {
                if (_notifier != null)
                {
                    await _notifier.PerformerDeletedAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify about performer deletion {Id}", id);
            }
            return true;
        }

        private static PerformerDTO ToDto(Performer p)
            => new()
            {
                Id = p.Id,
                Name = p.Name,
                Url = p.Url
            };
    }
}
