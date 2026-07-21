using System;

namespace MvcMoviesCore.Models
{
    public class AlsoKnownAs
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}