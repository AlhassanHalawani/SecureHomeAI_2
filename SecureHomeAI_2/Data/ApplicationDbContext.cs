using Microsoft.EntityFrameworkCore;
using SecureHomeAI_2.Models;

namespace SecureHomeAI_2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}

