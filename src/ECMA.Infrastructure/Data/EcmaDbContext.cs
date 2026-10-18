using ECMA.Domain.Entities;
using ECMA.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ECMA.Infrastructure.Data;

public sealed class EcmaDbContext(DbContextOptions<EcmaDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}