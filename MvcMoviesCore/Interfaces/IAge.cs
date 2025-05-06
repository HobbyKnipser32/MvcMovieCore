using System;

namespace MvcMoviesCore.Interfaces
{
    public interface IAge
    {
        public string ActorsAge { get; set; }
        public string GetActorsAgeInMovie(DateTime? birthDay, DateTime? yearOfPublication);
        public string GetActorsAge(DateTime? birthDay, DateTime? obit);
    }
}