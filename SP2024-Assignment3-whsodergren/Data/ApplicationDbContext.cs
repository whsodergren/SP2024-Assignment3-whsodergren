using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SP2024_Assignment3_whsodergren.Models;

namespace SP2024_Assignment3_whsodergren.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SP2024_Assignment3_whsodergren.Models.Movie> Movie { get; set; } = default!;
        public DbSet<SP2024_Assignment3_whsodergren.Models.Actor> Actor { get; set; } = default!;
        public DbSet<SP2024_Assignment3_whsodergren.Models.MovieActor> MovieActor { get; set; } = default!;

    }
}
