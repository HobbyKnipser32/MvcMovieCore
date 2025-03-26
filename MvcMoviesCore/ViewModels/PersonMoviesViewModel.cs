using System;

namespace MvcMoviesCore.ViewModels
{
    public class PersonMoviesViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Erscheinungsjahr { get; set; }

        public string Alter { get; set; }

        public string Role { get; set; }

        public string Praxis { get; set; }

        public string OnWatch { get; set; }

        public string Bewertung { get; set; }

        public string Laufzeit { get; set; }
    }
}