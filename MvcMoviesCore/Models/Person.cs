namespace MvcMoviesCore.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Person")]
    public partial class Person : Age
    {
        public Person()
        {
            MoviesPerson = new HashSet<MoviesPerson>();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid SexId { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [Column(TypeName = "datetime2")]
        public DateTime? Birthday { get; set; }

        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [Column(TypeName = "datetime2")]
        public DateTime? Obit { get; set; }

        public Guid? NationalityId { get; set; }

        public decimal? Height { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0}")]
        public decimal? Weight { get; set; }

        public int? Classification { get; set; }

        public Guid PersonTypesId { get; set; }

        public virtual ICollection<MoviesPerson> MoviesPerson { get; set; }

        public virtual PersonTypes PersonType { get; set; }

        public virtual Sex Sex { get; set; }

        public virtual Nationality Nationality { get; set; }
    }
}