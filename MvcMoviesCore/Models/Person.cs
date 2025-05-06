namespace MvcMoviesCore.Models
{
    using MvcMoviesCore.Classes;
    using MvcMoviesCore.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Person")]
    public partial class Person : IAge, IBMI
    {
        public Person()
        {
            MoviesPerson = new HashSet<MoviesPerson>();
        }

        public Guid Id { get; set; }

        [Required(ErrorMessage = "Der Name ist erforderlich!")]
        public string? Name { get; set; }

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

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:n}")]
        [RegularExpression(@"^\d{1}\,?\d{0,2}$", ErrorMessage = "Valid Decimal number with maximum  decimal places.")]
        public decimal? Height { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0}")]
        public decimal? Weight { get; set; }

        public int? Classification { get; set; }

        public string? CupSize { get; set; }

        public bool? FakeBoobs { get; set; } = false;

        public Guid PersonTypesId { get; set; }

        public int? StartOfBusiness { get; set; }

        public int? EndOfBusiness { get; set; }

        public string? Image { get; set; }

//        [NotMapped]
//        public string BMI
//        {
//            get
//            {
//                if (Height == null || Weight == null)
//                    return string.Empty;

//                var bmi = Weight.GetValueOrDefault() / (Height.GetValueOrDefault() * Height.GetValueOrDefault());
//#if DEBUG
//                return Math.Round(bmi, 2).ToString();
//#else
//                return Math.Round(bmi, 0).ToString();
//#endif
//            }
//        }

        public virtual ICollection<PersonImage> PersonImages { get; set; }

        public virtual ICollection<MoviesPerson> MoviesPerson { get; set; }

        public virtual PersonTypes PersonType { get; set; }

        public virtual Sex Sex { get; set; }

        public virtual Nationality Nationality { get; set; }

        #region interface member

        [NotMapped]
        public string ActorsAge { get; set; }
        
        [NotMapped]
        public int? Value { get; set; }

        public string GetActorsAge(DateTime? birthDay, DateTime? obit)
        {
            var age = new Age();
            return age.GetActorsAge(birthDay, obit);
        }

        public string GetActorsAgeInMovie(DateTime? birthDay, DateTime? yearOfPublication)
        {
            var age = new Age();
            return age.GetActorsAgeInMovie(birthDay, yearOfPublication);
        }

        public int? GetBMI(decimal? height, decimal? weight)
        {
            var bmi = new BMI();
            return bmi.GetBMI(height, weight);
        }

        public int? GetBMI(int? feet, int? inch, decimal? lbs)
        {
            var bmi = new BMI();
            return bmi.GetBMI(feet, inch, lbs);
        }

        #endregion
    }
}