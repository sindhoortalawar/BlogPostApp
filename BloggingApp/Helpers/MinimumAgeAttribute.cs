using System.ComponentModel.DataAnnotations;

namespace BloggingApp.Helpers
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
            ErrorMessage = $"You must be {minimumAge} years old to register.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            DateOnly dateOfBirth;
            if (value is DateOnly explicitDate)
            {
                dateOfBirth = explicitDate;
            }
            else if (value is DateTime dateTime)
            {
                dateOfBirth = DateOnly.FromDateTime(dateTime);
            }
            else
            {
                return new ValidationResult("Invalid Date of Birth format.");
            }
            var today = DateOnly.FromDateTime(DateTime.Today);

            var age = today.Year - dateOfBirth.Year;

            if(dateOfBirth > today.AddYears(-age))
            {
                age--;
            }
            if(age >= _minimumAge)
            {
                return ValidationResult.Success;
            }


            return new ValidationResult(ErrorMessage);


        }


    }
}
