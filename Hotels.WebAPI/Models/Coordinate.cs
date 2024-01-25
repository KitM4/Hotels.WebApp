using System.Reflection;

namespace Hotels.WebAPI.Models;

public record Coordinate(double Latilude, double Longitude)
{
    public static bool TryParse(string input, out Coordinate? coordinate)
    {
        coordinate = default;
        string[] splitArray = input.Split(',', 2);
        
        if (splitArray.Length != 2 || !double.TryParse(splitArray[0], out double lat) || !double.TryParse(splitArray[1], out double lon))
            return false;

        coordinate = new(lat, lon);
        return true;
    }

    public static Coordinate? Bind(HttpContext context, ParameterInfo parameter)
    {
        string input = context.GetRouteValue(parameter.Name!) as string ?? string.Empty;
        return TryParse(input, out Coordinate? coordinate) ? coordinate : null;
    }
}