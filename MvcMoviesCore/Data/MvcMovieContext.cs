using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace MvcMoviesCore.Models
{
    public class MvcMovieCoreContext : DbContext
    {
        public MvcMovieCoreContext()
        {
        }

        public MvcMovieCoreContext(DbContextOptions<MvcMovieCoreContext> options) : base(options)
        {
        }

        public DbSet<Genre> Genre { get; set; }

        public DbSet<Movies> Movies { get; set; }

        public DbSet<MoviesPerson> MoviesPerson { get; set; }

        public DbSet<Person> Person { get; set; }

        public DbSet<PersonTypes> PersonType { get; set; }

        public DbSet<RecordCarrier> RecordCarrier { get; set; }

        public DbSet<Sex> Sex { get; set; }

        public DbSet<StorageLocation> StorageLocation { get; set; }

        public DbSet<Nationality> Nationalities { get; set; }

        public DbSet<Scenes> Scenes { get; set; }

        public DbSet<MovieRole> MovieRole { get; set; }
    }
}