using Hotels.WebAPI.Data;
using Hotels.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using IServiceScope scopeService = app.Services.CreateScope();
    HotelDbContext dbContext = scopeService.ServiceProvider.GetRequiredService<HotelDbContext>();
    dbContext.Database.EnsureCreated();
}

#region API's

app.MapGet("/hotels", async (HotelDbContext dbContext) => await dbContext.Hotels.ToListAsync());
app.MapGet("/hotels/{id}", async (int id, HotelDbContext dbContext) =>
{
    Hotel? hotel = await dbContext.Hotels.FirstOrDefaultAsync(h => h.Id == id);
    return hotel == null ? Results.NotFound() : Results.Ok(hotel); 
});
app.MapPost("/hotels/create/", async ([FromBody] Hotel hotel, HotelDbContext dbContext, HttpResponse response) =>
{
    dbContext.Hotels.Add(hotel);

    await dbContext.SaveChangesAsync();
    return Results.Created($"/hotels/{hotel.Id}", hotel);
});
app.MapPut("/hotels/edit/", async ([FromBody] Hotel hotel, HotelDbContext dbContext) =>
{
    Hotel? hotelFromDb = await dbContext.Hotels.FindAsync(new object[] { hotel.Id });

    if (hotelFromDb == null)
        return Results.NotFound();

    hotelFromDb.Name = hotel.Name;
    hotelFromDb.Latitude = hotel.Latitude;
    hotelFromDb.Longitude = hotel.Longitude;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("hotels/{id}", async (int id, HotelDbContext dbContext) =>
{
    Hotel? hotelFromDb = await dbContext.Hotels.FindAsync(new object[] { id });

    if (hotelFromDb == null)
        return Results.NotFound();

    dbContext.Hotels.Remove(hotelFromDb);

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

#endregion

app.UseHttpsRedirection();

app.Run();