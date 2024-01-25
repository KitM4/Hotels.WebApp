using Hotels.WebAPI.Models;

namespace Hotels.WebAPI.Interfaces.Repository;

public interface IHotelRepository
{
    public Task<List<Hotel>> GetHotelsAsync();

    public Task<List<Hotel>> GetHotelsAsync(string name);

    public Task<List<Hotel>> GetHotelsAsync(Coordinate coordinate);

    public Task<Hotel?> GetHotelAsync(int hotelId);

    public Task InsertHotelAsync(Hotel hotel);

    public Task UpdateHotelAsync(Hotel hotel);

    public Task DeleteHotelAsync(int hotelId);

    public Task SaveAsync();
}