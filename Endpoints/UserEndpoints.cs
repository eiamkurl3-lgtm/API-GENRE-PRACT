using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Fitlers;
using MinimalApiMovies.Repositories;
using MinimalApiMovies.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MinimalApiMovies.Endpoints
{
    public static class UserEndpoints
    {
        public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
        {
            //group.MapGet("/", GetAllUsers);
            //group.MapGet("/{id:int}", GetUserById);
            //group.MapPost("/", CreateUser);
            //group.MapPut("/{id:int}", UpdateUser);
            //group.MapDelete("/{id:int}", DeleteUser);


            group.MapPost("/register", Register).AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
            group.MapPost("/login", Login)
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
            group.MapPost("/MakeAdmin",MakeAdmin)
                //.RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
            group.MapPost("/RemoveAdmin",RemoveAdmin)
                //.RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>> ();
            return group;
        }

        //[OutputCache(Duration = 60)]

        static async Task<Results<Ok<AuthenticationResponseDTO>,
            BadRequest<IEnumerable<IdentityError>>>> Register(UserCredentialsDTO userCredentialsDTO,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var user = new IdentityUser
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email
            };

            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);

            if (result.Succeeded)
            {
                var authenticationResponse =
                    await BuildToken(userCredentialsDTO, configuration, userManager);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                return TypedResults.BadRequest(result.Errors);
            }
        }

        private async static Task<AuthenticationResponseDTO>
            BuildToken(UserCredentialsDTO userCredentialsDTO,
            IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim("email", userCredentialsDTO.Email),
                new Claim("Whatever I want", "this is a value")
            };

            //var user = await userManager.FindByNameAsync(userCredentialsDTO.Email);
            //var claimsFromDB = await userManager.GetClaimsAsync(user!);

            //claims.AddRange(claimsFromDB);

            var key = KeysHandler.GetKey(configuration).First();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            //var expiration = DateTime.UtcNow.AddMinutes(30);      

            var securityToken = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiration, signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration
            };
        }

        static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<string>>> Login(
            UserCredentialsDTO userCredentialsDTO,
            [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager,
            IConfiguration configuration
            )
        {
            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if (user is null)
            {
                return TypedResults.BadRequest("There was a problem with the email or the password");
            }

            var results = await signInManager.CheckPasswordSignInAsync(user,
                userCredentialsDTO.Password, lockoutOnFailure: false);

            if (results.Succeeded)
            {
                var authenticationResponse =
                   await BuildToken(userCredentialsDTO, configuration, userManager);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                return TypedResults.BadRequest("There was a problem with the email or the password");
            }
        }

        static async Task<Results<NoContent, NotFound>> MakeAdmin(EditClaimDTO editClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(user, new Claim("isadmin", "true"));
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> RemoveAdmin(EditClaimDTO editClaimDTO,
           [FromServices] UserManager<IdentityUser> userManager)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.RemoveClaimAsync(user, new Claim("isadmin", "true"));
            return TypedResults.NoContent();
        }

        //static async Task<Ok<List<UserDTO>>> GetAllUsers(IUserRepository repository, IMapper mapper)
        //{
        //    var users = await repository.GetAll();
        //    var userDTOs = mapper.Map<List<UserDTO>>(users);
        //    return TypedResults.Ok(userDTOs);
        //}

        //[OutputCache(Duration = 60)]
        //static async Task<Results<Ok<UserDTO>, NotFound>> GetUserById(int id, IUserRepository repository, IMapper mapper)
        //{
        //    var user = await repository.GetById(id);
        //    if (user == null)
        //    {
        //        return TypedResults.NotFound();
        //    }
        //    var userDTO = mapper.Map<UserDTO>(user);
        //    return TypedResults.Ok(userDTO);
    }

        //static async Task<Results<Created<UserDTO>, NotFound>> CreateUser(CreateUserDTO createUserDTO, IUserRepository repository, IMapper mapper)
        //{
        //    var user = mapper.Map<User>(createUserDTO);
        //    var id = await repository.Create(user);
        //    user.Id = id;
        //    var userDTO = mapper.Map<UserDTO>(user);
        //    return TypedResults.Created($"/User/{id}", userDTO);
        //}

        //static async Task<Results<Ok, NotFound>> UpdateUser(int id, CreateUserDTO updateUserDTO, IUserRepository repository, IMapper mapper)
        //{
        //    var exists = await repository.Exists(id);
        //    if (!exists)
        //    {
        //        return TypedResults.NotFound();
        //    }
        //    var user = mapper.Map<User>(updateUserDTO);
        //    user.Id = id;
        //    await repository.Update(user);
        //    return TypedResults.Ok();
        //}

        //static async Task<Results<Ok, NotFound>> DeleteUser(int id, IUserRepository repository)
        //{
        //    var exists = await repository.Exists(id);
        //    if (!exists)
        //    {
        //        return TypedResults.NotFound();
        //    }
        //    await repository.Delete(id);
        //    return TypedResults.Ok();
        //}
    //}
}
