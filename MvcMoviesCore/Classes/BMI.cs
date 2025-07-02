using MvcMoviesCore.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMoviesCore.Classes
{
    public class BMI : IBMI
    {
        /// <summary>
        /// The Value of a BMI of this person.
        /// </summary>
        [NotMapped]
        public decimal? Value { get; set; }

        /// <summary>
        /// Get the bmi of a person
        /// </summary>
        /// <param name="height">The height in meter. </param>
        /// <param name="weight">The weight in kg.</param>
        /// <returns>The bmi.</returns>
        public decimal? GetBMI(decimal? height, decimal? weight, int decimals = 0)
        {
            if (height == null || height == 0 || weight == null || weight == 0)
                return null;

            return Math.Round((weight / (height * height)).GetValueOrDefault(), decimals);
        }

        public decimal? GetBMI(int? feet, int? inch, decimal? lbs, int decimals = 0)
        {
            if (feet == null || feet == 0 || lbs == null || lbs == 0)
                return null;

            var weight = lbs.GetValueOrDefault() * 0.45359m;
            var height = feet.GetValueOrDefault() * 0.3048m;
            if (inch.HasValue) height += inch.GetValueOrDefault() * 0.0254m;

            return GetBMI(height, weight, decimals);
        }
    }
}