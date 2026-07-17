namespace MinimalApiMovies.DTOs
{
    public class CreateActorDTO
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime BirthDate { get; set; }

        public string ProfilePicture { get; set; } = null!;
    }
}
