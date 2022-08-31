using System;
using System.Collections.Generic;

namespace MvcMoviesCore.Models
{
    public class Scenes
    {
        public Guid Id { get; set; }

        public int Scene { get; set; }

        public Guid MoviesPersonsId { get; set; }

        //public virtual ICollection<MoviesPerson> MoviePersons { get; set; }
    }
}