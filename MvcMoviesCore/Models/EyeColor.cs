using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public class EyeColor
    {
        public Guid Id { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public int Count { get; set; }
    }
}