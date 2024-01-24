using Hotels.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotels.WebAPI.Data;

public class HotelDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
}