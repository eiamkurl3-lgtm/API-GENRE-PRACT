using AutoMapper;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Genre mappings
            CreateMap<CreateGenreDTO, Genre>();
            CreateMap<Genre, GenreDTO>();

            // User mappings
            CreateMap<CreateUserDTO, User>();
            CreateMap<User, UserDTO>();

            // Actor mappings
            CreateMap<CreateActorDTO, Actor>()
                .ForMember(p=> p.ProfilePicture,options=> options.Ignore());
            CreateMap<Actor, ActorDTO>();

            // Movies mappings
            CreateMap<CreateMoviesDTO, Movies>()
                .ForMember(p => p.Poster, options => options.Ignore());
            CreateMap<Movies, MoviesDTO>()
                 .ForMember(x => x.Genres, entity =>
                             entity.MapFrom(p => p.GenresMovies.Select(
                                 gm => new GenreDTO
                                 {
                                     Id = gm.GenreId,
                                     Name = gm.genre.Name
                                 })))
                 .ForMember(x => x.Actors, entity =>
                             entity.MapFrom(p => p.ActorsMovies.Select(
                                 am => new ActorMovieDTO
                                 {
                                     Id = am.ActorId,
                                     Name = am.Actor.Name,
                                     character = am.Character
                                 })));

            // Comments mappings
            CreateMap<CreateCommentsDTO, Comments>();
            CreateMap<Comments, CommentsDTO>();

            CreateMap<AssignActorMovieDTO, ActorMovie>();
        }
    }
}
