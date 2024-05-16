using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    [Table("Nationality")]
    public partial class Nationality
    {
        public Nationality()
        {
            Person = [];
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Person> Person { get; set; }
    }
}