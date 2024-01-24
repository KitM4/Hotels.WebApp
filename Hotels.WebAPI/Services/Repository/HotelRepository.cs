using Hotels.WebAPI.Data;
using Hotels.WebAPI.Models;
using Hotels.WebAPI.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Hotels.WebAPI.Services.Repository;

public class HotelRepository(HotelDbContext dbContext) : IHotelRepository
{
    private readonly HotelDbContext _dbContext = dbContext;
    private bool _disposed = false;

    public Task<List<Hotel>> GetHotelsAsync() =>
        _dbContext.Hotels.ToListAsync();

    public async Task<Hotel?> GetHotelAsync(int hotelId) =>
        await _dbContext.Hotels.FindAsync(new object[] { hotelId });

    public async Task InsertHotelAsync(Hotel hotel) =>
        await _dbContext.Hotels.AddAsync(hotel);

    public async Task UpdateHotelAsync(Hotel hotel)
    {
        Hotel? hotelFromDb = await _dbContext.Hotels.FindAsync(new object[] { hotel.Id });

        if (hotelFromDb != null)
        {
            hotelFromDb.Longitude = hotel.Longitude;
            hotelFromDb.Latitude = hotel.Latitude;
            hotelFromDb.Name = hotel.Name;
        }
    }

    public async Task DeleteHotelAsync(int hotelId)
    {
        Hotel? hotelFromDb = await _dbContext.Hotels.FindAsync(new object[] { hotelId });

        if (hotelFromDb != null)
            _dbContext.Hotels.Remove(hotelFromDb);
    }

    public async Task SaveAsync() =>
        await _dbContext.SaveChangesAsync();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _dbContext.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}