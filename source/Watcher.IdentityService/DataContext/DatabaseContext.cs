using Microsoft.EntityFrameworkCore;
using Watcher.IdentityService.Models;

namespace Watcher.IdentityService.DataContext;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}