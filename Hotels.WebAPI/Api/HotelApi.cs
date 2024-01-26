using Hotels.WebAPI.Models;
using Hotels.WebAPI.ViewModels;
using Hotels.WebAPI.Interfaces;
using Hotels.WebAPI.Utils.Extensions;
using Hotels.WebAPI.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Hotels.WebAPI.Api;

public class HotelApi : IApi
{
    public void Register(WebApplication app)
    {
        app.MapGet("/hotels", Get)
            .Produces<List<Hotel>>(StatusCodes.Status200OK)
            .WithName("GetAllHotels")
            .WithTags("Getters");

        app.MapGet("/hotels/search/name/{query}", GetByName)
            .Produces<List<Hotel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("SearchHotelsByName")
            .WithTags("Getters")
            .ExcludeFromDescription();

        app.MapGet("/hotels/search/location/{coordinate}", GetByCoords)
            .Produces<List<Hotel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("SearchHotelsByLocation")
            .WithTags("Getters")
            .ExcludeFromDescription();

        app.MapGet("/hotels/{id}", GetById)
            .Produces<Hotel>(StatusCodes.Status200OK)
            .WithName("GetHotel")
            .WithTags("Getters");

        app.MapPost("/hotels/create", Create)
            .Accepts<Hotel>("application/json")
            .Produces<Hotel>(StatusCodes.Status201Created)
            .WithName("CreateHotel")
            .WithTags("Creators");

        app.MapPut("/hotels/edit", Edit)
            .Accepts<Hotel>("application/json")
            .WithName("EditHotel")
            .WithTags("Updaters");

        app.MapDelete("hotels/delete/{id}", Delete)
            .WithName("DeleteHotel")
            .WithTags("Deleters");
    }

    [Authorize]
    [HttpGet("/hotels")]
    private async Task<IResult> Get(IHotelRepository repository) =>
        Results.Extensions.Xml(await repository.GetHotelsAsync());

    [Authorize]
    [HttpGet("/hotels/search/name/{query}")]
    private async Task<IResult> GetByName(string query, IHotelRepository repository) =>
        await repository.GetHotelsAsync(query) is IEnumerable<Hotel> hotels ? Results.Ok(hotels) : Results.NotFound(Array.Empty<Hotel>());

    [Authorize]
    [HttpGet("/hotels/search/location/{coordinate}")]
    private async Task<IResult> GetByCoords(Coordinate coordinate, IHotelRepository repository) =>
        await repository.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels ? Results.Ok(hotels) : Results.NotFound(Array.Empty<Hotel>());

    [Authorize]
    [HttpGet("/hotels/{id}")]
    private async Task<IResult> GetById(int id, IHotelRepository repository) =>
        await repository.GetHotelAsync(id) is Hotel hotel ? Results.Ok(hotel) : Results.NotFound();

    [Authorize]
    [HttpPost("/hotels/create")]
    private async Task<IResult> Create([FromBody] HotelViewModel hotelViewMode, IHotelRepository repository)
    {
        string? name = hotelViewMode.Name;
        double latitude = hotelViewMode.Latitude;
        double longitude = hotelViewMode.Longitude;

        if (string.IsNullOrWhiteSpace(name) || latitude == default || longitude == default)
            return Results.BadRequest();

        Hotel hotel = new()
        {
            Name = name,
            Latitude = latitude,
            Longitude = longitude,
        };

        await repository.InsertHotelAsync(hotel);
        await repository.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    }

    [Authorize]
    [HttpPut("/hotels/edit")]
    private async Task<IResult> Edit([FromBody] Hotel hotel, IHotelRepository repository)
    {
        await repository.UpdateHotelAsync(hotel);
        await repository.SaveAsync();
        return Results.NoContent();
    }

    [Authorize]
    [HttpDelete("hotels/delete/{id}")]
    private async Task<IResult> Delete(int id, IHotelRepository repository)
    {
        await repository.DeleteHotelAsync(id);
        await repository.SaveAsync();
        return Results.NoContent();
    }
}