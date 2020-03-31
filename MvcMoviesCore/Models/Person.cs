namespace MvcMoviesCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Person")]
    public partial class Person : Age
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Person()
        {
            MoviesPerson = new HashSet<MoviesPerson>();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid SexId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? Birthday { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? Obit { get; set; }

        public Guid? NationalityId { get; set; }

        public decimal? Height { get; set; }

        public decimal? Weight { get; set; }

        public Guid PersonTypesId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MoviesPerson> MoviesPerson { get; set; }

        public virtual PersonTypes PersonType { get; set; }

        public virtual Sex Sex { get; set; }

        public virtual Nationality Nationality { get; set; }
    }
}