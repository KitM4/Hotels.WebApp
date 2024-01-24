namespace Hotels.WebAPI.Models;

public class Hotel
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required double Latitude { get; set; }

    public required double Longitude { get; set;}
}