using Microsoft.AspNetCore.Identity;

namespace MinimalApiMovies.Entities
{
    public class Comments
    {
        public int id { get; set; }
        public string Body { get; set; } = null!;
        public int MovieId { get; set; }

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;
    }
}
