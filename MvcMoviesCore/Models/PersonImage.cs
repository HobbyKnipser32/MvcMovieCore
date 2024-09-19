using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieExcelImporter._02.Entities
{
    [Table("PersonImage")]
    public class PersonImage
    {
        public Guid Id { get; set; }

        public Guid PersonId { get; set; }

        public string Name { get; set; }

        public bool IsMain { get; set; }
    }
}