using System;

namespace MvcMoviesCore.Models
{
    public class MovieRole
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsAdult { get; set; } = false;
    }
}