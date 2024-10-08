using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    [Table("PersonImage")]
    public class PersonImage
    {
        public Guid Id { get; set; }

        public Guid PersonId { get; set; }

        public string Name { get; set; }

        public bool IsMain { get; set; }

        public bool IsDeleted { get; set; }

        public int Number { get; set; }

        public DateTime CreatetAt { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}