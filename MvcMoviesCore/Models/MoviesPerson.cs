namespace MvcMoviesCore.Models
{
    public partial class MoviesPerson
    {
        public System.Guid Id { get; set; }

        public System.Guid MoviesId { get; set; }

        public System.Guid PersonId { get; set; }

        public string Practices { get; set; }

        public virtual Movies Movies { get; set; }

        public virtual Person Person { get; set; }
    }
}