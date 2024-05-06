using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public partial class Movies : Age
    {
        public Movies()
        {
            this.MoviesPerson = new HashSet<MoviesPerson>();
        }

        public Guid Id { get; set; }

        [Required(ErrorMessage = "Der Filmtitel ist erforderlich")]
        public string Name { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy}")]
        public DateTime? YearOfPublication { get; set; }

        public Guid GenreId { get; set; }

        public Guid? RecordCarrierId { get; set; }

        public bool? InStock { get; set; }

        public Guid? StorageLocationId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime? Added { get; set; }

        public string OnWatch { get; set; }

        public string Remark { get; set; }

        public bool? Adult { get; set; }

        public bool? ThreeD { get; set; }

        public string Owner { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IMDB { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Ranking { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:D}")]
        public DateTime? LastView { get; set; }

        public DateTime? CreateDate { get; set; }

        public string CreateUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public string ChangeUser { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RunTime { get; set; }

        [NotMapped]
        public List<ViewModelScenes> Scenes { get; set; }

        [Required]
        public virtual Genre Genre { get; set; }

        public virtual RecordCarrier RecordCarrier { get; set; }

        public virtual StorageLocation StorageLocation { get; set; }

        public virtual ICollection<MoviesPerson> MoviesPerson { get; set; }
    }
}