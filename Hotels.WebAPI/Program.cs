using System.Text;
using Hotels.WebAPI.Data;
using Hotels.WebAPI.Models;
using Hotels.WebAPI.Services;
using Hotels.WebAPI.Interfaces;
using Hotels.WebAPI.Utils.Extensions;
using Hotels.WebAPI.Services.Repository;
using Hotels.WebAPI.Interfaces.Repository;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    string key = builder.Configuration["Jwt:Key"] ?? "KitM4-1111111111111111111111111111111111111111111111111111111111111111111";

    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

WebApplication app = builder.Build();

app.UseAuthorization();
app.UseAuthentication();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using IServiceScope scopeService = app.Services.CreateScope();
    HotelDbContext dbContext = scopeService.ServiceProvider.GetRequiredService<HotelDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/login", [AllowAnonymous] (HttpContext context, ITokenService tokenService, IUserRepository userRepository) =>
{
    string? name = context.Request.Query["name"][0];
    string? password = context.Request.Query["password"][0];

    if (name != null)
    {
        User? user = userRepository.GetUser(name);
        if (user != null && password == user.Password)
        {
            string? key = builder.Configuration["Jwt:Key"];
            string? issuer = builder.Configuration["Jwt:Issuer"];

            if (key != null && issuer != null)
                return Results.Ok(tokenService.BuildToken(key, issuer, user));
        }
        
        return Results.Unauthorized();
    }

    return Results.BadRequest();
});

app.MapGet("/hotels", [Authorize] async (IHotelRepository repository) =>
    Results.Extensions.Xml(await repository.GetHotelsAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/search/name/{query}",
    [Authorize] async (string query, IHotelRepository repository) =>
        await repository.GetHotelsAsync(query) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByName")
    .WithTags("Getters")
    .ExcludeFromDescription();

app.MapGet("/hotels/search/location/{coordinate}",
    [Authorize] async (Coordinate coordinate, IHotelRepository repository) =>
        await repository.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByLocation")
    .WithTags("Getters")
    .ExcludeFromDescription();

app.MapGet("/hotels/{id}", [Authorize] async (int id, IHotelRepository repository) =>
{
    Hotel? hotel = await repository.GetHotelAsync(id);
    return hotel == null ? Results.NotFound() : Results.Ok(hotel);

}).Produces<Hotel>(StatusCodes.Status200OK).WithName("GetHotel").WithTags("Getters");

app.MapPost("/hotels/create/", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository repository) =>
{
    await repository.InsertHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);

}).Accepts<Hotel>("application/json").Produces<Hotel>(StatusCodes.Status201Created).WithName("CreateHotel").WithTags("Creators");

app.MapPut("/hotels/edit/", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository repository) =>
{
    await repository.UpdateHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.NoContent();

}).Accepts<Hotel>("application/json").WithName("EditHotel").WithTags("Updaters");

app.MapDelete("hotels/{id}", [Authorize] async (int id, IHotelRepository repository) =>
{
    await repository.DeleteHotelAsync(id);
    await repository.SaveAsync();
    return Results.NoContent();

}).WithName("DeleteHotel").WithTags("Deleters");

app.Run();