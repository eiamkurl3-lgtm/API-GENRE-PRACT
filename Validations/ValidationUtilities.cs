using MinimalApiMovies.DTOs;

namespace MinimalApiMovies.Validations
{
    public static class ValidationUtilities
    {
        public static string NonEmptyMessage = "The Field {PropertyName} is Required";
        public static string MessageMaxLength = "Field {PropertyName} must be less than {MaxLength} Characters";
        public static string UpperCaseMessage = "firstletter must be uppercase {PropertyName}";
        //public static string ExistsMessage = "The field {PropertyName} already exists";

        public static string GreaterThanDate(DateTime value) => $"The field {nameof(CreateActorDTO.BirthDate)} must be greater than or equal to" + value.ToString("yyyyy-MM-dd"); 

        public static bool FirstLetterIsUppercase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var firstletter = value[0].ToString();
            return firstletter == firstletter.ToUpper();
        }
    }
}
