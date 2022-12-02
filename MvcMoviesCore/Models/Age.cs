using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Models
{
    public abstract class Age
    {
        /// <summary>
        /// The current Age of this Person
        /// </summary>
        [NotMapped]
        public string ActorsAge { get; set; }

        /// <summary>
        /// The Persons Age to the Movie start
        /// </summary>
        /// <param name="birthDay">Birthday of the Person</param>
        /// <param name="yearOfPublication">Start of the movie</param>
        /// <returns></returns>
        public string GetActorsAgeInMovie(DateTime? birthDay, DateTime? yearOfPublication)
        {
            var age = "(-)";
            if (birthDay != null && yearOfPublication != null)
                return GetAge(birthDay, yearOfPublication);
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
                return obit != null ? $"{GetAge(birthDay, obit )} (†)" : GetAge(birthDay, DateTime.Now);

            return age;
        }

        /// <summary>
        /// Get the current age.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private string GetAge(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return "(-)";

            var localEndDate = new DateTime();
            var localStartDate = new DateTime();

            if (startDate != null)
                localStartDate = startDate.Value;
            if (endDate != null)
                localEndDate = endDate.Value;

            return (localEndDate.Year - localStartDate.Year - 1 +
                 (((localStartDate.Month < localEndDate.Month) ||
                  ((localStartDate.Month == localEndDate.Month) &&
                   (localStartDate.Day <= localEndDate.Day))) ? 1 : 0)).ToString();
        }
    }
}