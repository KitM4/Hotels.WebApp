using Hotels.WebAPI.Models;

namespace Hotels.WebAPI.Interfaces;

public interface ITokenService
{
    public string BuildToken(string key, string issuer, User user);
}