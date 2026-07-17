namespace MinimalApiMovies.Entities
{
    public class Actor
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime BirthDate { get; set; }

        public string ProfilePicture { get; set; } = null!;
    }
}
