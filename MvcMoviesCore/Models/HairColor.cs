using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public class HairColor
    {
        public Guid Id { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public int Count { get; set; }
    }
}