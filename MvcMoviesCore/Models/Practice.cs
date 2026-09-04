using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    [Table("Practice", Schema = "dbo")]
    public class Practice
    {
        public Guid Id { get; set; }
        public string Praxis { get; set; }
        public string Description { get; set; }
        public bool IsAdult { get; set; } = true;
    }
}