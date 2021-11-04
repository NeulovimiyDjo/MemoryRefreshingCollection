using Microsoft.EntityFrameworkCore;

namespace server.Models
{
    public class CharactersContext : DbContext
    {
        public DbSet<Character> Characters { get; set; }

        public CharactersContext(DbContextOptions<CharactersContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}