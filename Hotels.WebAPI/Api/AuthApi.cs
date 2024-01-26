using Hotels.WebAPI.Models;
using Hotels.WebAPI.Interfaces;
using Hotels.WebAPI.ViewModels;
using Hotels.WebAPI.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Hotels.WebAPI.Api;

public class AuthApi : IApi
{
    [AllowAnonymous]
    [HttpPost("/login")]
    public void Register(WebApplication app)
    {
        app.MapPost("/login", [AllowAnonymous] ([FromBody] LoginViewModel loginViewModel, ITokenService tokenService, IUserRepository userRepository) =>
        {
            string? name = loginViewModel.UserName;
            string? password = loginViewModel.Password;

            if (name != null)
            {
                User? user = userRepository.GetUser(name);
                if (user != null && password == user.Password)
                {
                    string? key = app.Configuration["Jwt:Key"];
                    string? issuer = app.Configuration["Jwt:Issuer"];

                    if (key != null && issuer != null)
                        return Results.Ok(tokenService.BuildToken(key, issuer, user));
                }

                return Results.Unauthorized();
            }

            return Results.BadRequest();
        });
    }
}