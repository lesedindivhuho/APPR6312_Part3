using Microsoft.EntityFrameworkCore;
using Part1.Models; // because of my Donation, Clothes, Food that are in Models/Donation folder

namespace Part1.Data
{
    public class Part1Context : DbContext
    {
        public Part1Context(DbContextOptions<Part1Context> options)
            : base(options)
        {
        }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Clothes> Clothes { get; set; }
        public DbSet<Food> Food { get; set; }
        public DbSet<ProjectUpdate> ProjectUpdates { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
    }
}
