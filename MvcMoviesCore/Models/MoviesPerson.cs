using System;

namespace MvcMoviesCore.Models
{
    public partial class MoviesPerson
    {
        public Guid Id { get; set; }

        public Guid MoviesId { get; set; }

        public Guid PersonId { get; set; }

        public Guid? MovieRoleId { get; set; }

        public string? Practices { get; set; }

        public virtual Movies Movies { get; set; }

        public virtual Person Person { get; set; }

        public virtual MovieRole MovieRole { get; set; }
    }
}