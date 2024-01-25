using Hotels.WebAPI.Data;
using Hotels.WebAPI.Models;
using Hotels.WebAPI.Services.Repository;
using Hotels.WebAPI.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using IServiceScope scopeService = app.Services.CreateScope();
    HotelDbContext dbContext = scopeService.ServiceProvider.GetRequiredService<HotelDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) =>
    Results.Ok(await repository.GetHotelsAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/search/name/{query}",
    async (string query, IHotelRepository repository) =>
        await repository.GetHotelsAsync(query) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotels")
    .WithTags("Getters")
    .ExcludeFromDescription();

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) =>
{
    Hotel? hotel = await repository.GetHotelAsync(id);
    return hotel == null ? Results.NotFound() : Results.Ok(hotel);

}).Produces<Hotel>(StatusCodes.Status200OK).WithName("GetHotel").WithTags("Getters");

app.MapPost("/hotels/create/", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
{
    await repository.InsertHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);

}).Accepts<Hotel>("application/json").Produces<Hotel>(StatusCodes.Status201Created).WithName("CreateHotel").WithTags("Creators");

app.MapPut("/hotels/edit/", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
{
    await repository.UpdateHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.NoContent();

}).Accepts<Hotel>("application/json").WithName("EditHotel").WithTags("Updaters");

app.MapDelete("hotels/{id}", async (int id, IHotelRepository repository) =>
{
    await repository.DeleteHotelAsync(id);
    await repository.SaveAsync();
    return Results.NoContent();

}).WithName("DeleteHotel").WithTags("Deleters");

app.UseHttpsRedirection();
app.Run();