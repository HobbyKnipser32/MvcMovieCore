using System;

namespace MvcMoviesCore.Models
{
    public class Scenes
    {
        public Guid Id { get; set; }

        public int Scene { get; set; }

        public Guid MoviesPersonsId { get; set; }
    }
}