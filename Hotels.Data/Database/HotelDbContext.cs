using Hotels.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotels.Data.Database;

public class HotelDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

}