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
using MinimalApiMovies.Services;
using MinimalApiMovies.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
                .RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<EditClaimDTO>>();

            group.MapPost("/RemoveAdmin",RemoveAdmin)
                .RequireAuthorization("isadmin")
                .AddEndpointFilter<ValidationFilter<EditClaimDTO>> ();


            group.MapGet("/renewtoken", Renew).RequireAuthorization();

            group.MapPost("/refresh", Refresh);

            group.MapPost("/logout", Logout);
            //.RequireAuthorization();
                
            group.MapPost("/logout-all", LogoutAll);
                //.RequireAuthorization();

            return group;
        }

        //[OutputCache(Duration = 60)]

        static async Task<Results<Ok<AuthenticationResponseDTO>,
            BadRequest<IEnumerable<IdentityError>>>> Register(UserCredentialsDTO userCredentialsDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
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
                    await BuildToken(userCredentialsDTO, configuration, userManager, refreshTokenRepository);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                return TypedResults.BadRequest(result.Errors);
            }
        }

        private async static Task<AuthenticationResponseDTO>
            BuildToken(UserCredentialsDTO userCredentialsDTO,
            IConfiguration configuration, UserManager<IdentityUser> userManager,
            IRefreshTokenRepository refreshTokenRepository)
        {
            var claims = new List<Claim>
            {
                new Claim("email", userCredentialsDTO.Email),
                new Claim("Whatever I want", "this is a value")
            };

            var user = await userManager.FindByNameAsync(userCredentialsDTO.Email);
            var claimsFromDB = await userManager.GetClaimsAsync(user!);

            claims.AddRange(claimsFromDB);

            var key = KeysHandler.GetKey(configuration).First();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(30);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiration, signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(7);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user!.Id,
                TokenHash = ComputeSha256Hash(refreshTokenValue),
                ExpiresAt = refreshTokenExpiration,
                CreatedAt = DateTime.UtcNow
            };

            await refreshTokenRepository.CreateAsync(refreshToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration,
                RefreshToken = refreshTokenValue,   
                RefreshTokenExpiration = refreshTokenExpiration
            };
        }

        private static string ComputeSha256Hash(string rawData)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }

        static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<string>>> Login(
            UserCredentialsDTO userCredentialsDTO,
            [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] IRefreshTokenRepository refreshTokenRepository,
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
                await refreshTokenRepository.CleanupForUserAsync(user.Id);
                var authenticationResponse =
                   await BuildToken(userCredentialsDTO, configuration, userManager, refreshTokenRepository);
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

        private static async Task<Results<NotFound, Ok<AuthenticationResponseDTO>>> Renew(
           IUsersService usersService, IConfiguration configuration,
           [FromServices] UserManager<IdentityUser> userManager,
           [FromServices] IRefreshTokenRepository refreshTokenRepository)
        {
            var user = await usersService.GetUser();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var usersCredential = new UserCredentialsDTO { Email = user.Email! };
            var response = await BuildToken(usersCredential, configuration, userManager, refreshTokenRepository);
            return TypedResults.Ok(response);
        }

        static async Task<Results<BadRequest<string>, Ok<AuthenticationResponseDTO>>> Refresh(
            RefreshTokenRequestDTO refreshTokenRequest,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            if (refreshTokenRequest?.RefreshToken is null)
            {
                return TypedResults.BadRequest("Refresh token is required");
            }

            var tokenHash = ComputeSha256Hash(refreshTokenRequest.RefreshToken);
            var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (storedToken is null)
            {
                return TypedResults.BadRequest("Invalid refresh token");
            }

            if (!storedToken.IsActive)
            {
                return TypedResults.BadRequest("Refresh token is expired or revoked");
            }

            var user = await userManager.FindByIdAsync(storedToken.UserId);

            if (user is null)
            {
                return TypedResults.BadRequest("User not found");
            }

            await refreshTokenRepository.RevokeAsync(storedToken.Id);

            var userCredential = new UserCredentialsDTO { Email = user.Email! };
            var response = await BuildToken(userCredential, configuration, userManager, refreshTokenRepository);
            return TypedResults.Ok(response);
        }

        static async Task<Results<NoContent, BadRequest<string>, NotFound>> Logout(
            RefreshTokenRequestDTO refreshTokenRequest,
            IUsersService usersService,
            [FromServices] IRefreshTokenRepository refreshTokenRepository)
        {
            if (refreshTokenRequest?.RefreshToken is null)
            {
                return TypedResults.BadRequest("Refresh token is required");
            }

            var user = await usersService.GetUser();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            var tokenHash = ComputeSha256Hash(refreshTokenRequest.RefreshToken);
            var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (storedToken is null)
            {
                return TypedResults.BadRequest("Invalid refresh token");
            }

            if (storedToken.UserId != user.Id)
            {
                return TypedResults.BadRequest("Refresh token does not belong to this user");
            }

            await refreshTokenRepository.RevokeAsync(storedToken.Id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> LogoutAll(
            IUsersService usersService,
            [FromServices] IRefreshTokenRepository refreshTokenRepository)
        {
            var user = await usersService.GetUser();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await refreshTokenRepository.RevokeAllForUserAsync(user.Id);
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
