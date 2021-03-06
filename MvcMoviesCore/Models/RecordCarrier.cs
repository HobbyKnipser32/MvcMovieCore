using System.Collections.Generic;

namespace MvcMoviesCore.Models
{
    public partial class RecordCarrier
    {
        public RecordCarrier()
        {
            this.Movies = new HashSet<Movies>();
        }
    
        public System.Guid Id { get; set; }

        public string Name { get; set; }
    
        public virtual ICollection<Movies> Movies { get; set; }
    }
}