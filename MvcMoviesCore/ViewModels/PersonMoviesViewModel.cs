using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Bewertung { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Laufzeit { get; set; }
    }
}