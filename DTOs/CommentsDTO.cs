namespace MinimalApiMovies.DTOs
{
    public class CommentsDTO
    {
        public int Id { get; set; }
        public string Body { get; set; } = null!;
        public int MovieId { get; set; }
    }
}