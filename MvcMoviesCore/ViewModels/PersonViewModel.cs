using Microsoft.AspNetCore.Http;
using MvcMoviesCore.Models;
using System.Collections.Generic;

namespace MvcMoviesCore.ViewModels
{
    public class PersonViewModel : Person
    {
        public PersonViewModel() { }

        public PersonViewModel(Person person)
        {
            Id = person.Id;
            Name = person.Name;
            SexId = person.SexId;
            Birthday = person.Birthday;
            Obit = person.Obit;
            NationalityId = person.NationalityId;
            Height = person.Height;
            Weight = person.Weight;
            Classification = person.Classification;
            CupSize = person.CupSize;
            FakeBoobs = person.FakeBoobs;
            PersonTypesId = person.PersonTypesId;
            StartOfBusiness = person.StartOfBusiness;
            EndOfBusiness = person.EndOfBusiness;
            Image = person.Image;
            ActorsAge = person.ActorsAge;
            Images = person.PersonImages;
        }

        public IFormFile SelectedFile { get; set; }

        public string PreviousPage { get; set; }

        public ICollection<PersonImage> Images { get; set; }
    }
}