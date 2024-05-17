using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public partial class Genre
    {
        public Genre() { }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsAdult { get; set; } = false;

        [NotMapped]
        public int Count { get; set; }

        public virtual ICollection<Movies> Movies { get; set; } = new HashSet<Movies>();
    }
}