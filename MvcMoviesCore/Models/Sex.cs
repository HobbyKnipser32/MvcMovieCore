using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public partial class Sex
    {
        public Sex() { }

        public System.Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [NotMapped]
        public int Count { get; set; }

        public virtual ICollection<Person> Person { get; set; } = new HashSet<Person>();
    }
}