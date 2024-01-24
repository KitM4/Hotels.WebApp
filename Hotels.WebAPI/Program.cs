using Hotels.WebAPI.Data;
using Hotels.WebAPI.Models;
using Hotels.WebAPI.Services.Repository;
using Hotels.WebAPI.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using IServiceScope scopeService = app.Services.CreateScope();
    HotelDbContext dbContext = scopeService.ServiceProvider.GetRequiredService<HotelDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) =>
    Results.Ok(await repository.GetHotelsAsync()));

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) =>
{
    Hotel? hotel = await repository.GetHotelAsync(id);
    return hotel == null ? Results.NotFound() : Results.Ok(hotel); 
});

app.MapPost("/hotels/create/", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
{
    await repository.InsertHotelAsync(hotel);
    await repository.SaveAsync();

    return Results.Created($"/hotels/{hotel.Id}", hotel);
});

app.MapPut("/hotels/edit/", async ([FromBody] Hotel hotel, IHotelRepository repository) =>
{
    await repository.UpdateHotelAsync(hotel);
    await repository.SaveAsync();
    return Results.NoContent();
});
app.MapDelete("hotels/{id}", async (int id, IHotelRepository repository) =>
{
    await repository.DeleteHotelAsync(id);
    await repository.SaveAsync();
    return Results.NoContent();
});

app.UseHttpsRedirection();
app.Run();