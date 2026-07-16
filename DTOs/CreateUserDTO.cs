namespace MinimalApiMovies.DTOs
{
    public class CreateUserDTO
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public int Age { get; set; }

        public string Gender { get; set; } = null!;

        public bool IsActive { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Name { get; set; } = null!;
    }
}
