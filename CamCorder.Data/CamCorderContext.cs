using CamCorder.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CamCorder.Data;

public class CamCorderContext(DbContextOptions<CamCorderContext> options) : DbContext(options)
{
    public DbSet<Performer> Performers => Set<Performer>();
    public DbSet<Recording> Recordings => Set<Recording>();
}
