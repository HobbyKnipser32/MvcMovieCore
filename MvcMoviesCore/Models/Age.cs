using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public abstract class Age
    {
        /// <summary>
        /// The current Age this Person
        /// </summary>
        [NotMapped]
        public string ActorsAge { get; set; }

        /// <summary>
        /// The Persons Age to the Movie start
        /// </summary>
        /// <param name="birthDay">Birthday of the Person</param>
        /// <param name="yearOfPublication">Start of the movie</param>
        /// <returns></returns>
        public string GetActorsMovieAge(DateTime? birthDay, DateTime? yearOfPublication)
        {
            var age = "(-)";
            if (birthDay != null && yearOfPublication != null)
                return GetAge(yearOfPublication, birthDay);
            return age;
        }

        /// <summary>
        /// The current age of the Person or the age by the dead
        /// </summary>
        /// <param name="birthDay">Birthday of the person</param>
        /// <param name="obit">Obit of the person</param>
        /// <returns></returns>
        public string GetActorsAge(DateTime? birthDay, DateTime? obit)
        {
            var age = "(-)";

            if (birthDay != null)
                return obit != null ? $"{GetAge(obit, birthDay)}  (†)" : GetAge(DateTime.Now, birthDay);

            return age;
        }

        /// <summary>
        /// Get the current age.
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        private string GetAge(DateTime? endDate, DateTime? startDate) =>
            // ReSharper disable once PossibleInvalidOperationException
            ((int) ((endDate - startDate)?.TotalDays / 365.25)).ToString();
    }
}