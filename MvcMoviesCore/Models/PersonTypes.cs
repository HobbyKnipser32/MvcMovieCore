using System.Collections.Generic;

namespace MvcMoviesCore.Models
{
    public partial class PersonTypes
    {
        public PersonTypes()
        {
            this.Person = new HashSet<Person>();
        }
    
        public System.Guid Id { get; set; }

        public string Name { get; set; }
    
        public virtual ICollection<Person> Person { get; set; }
    }
}