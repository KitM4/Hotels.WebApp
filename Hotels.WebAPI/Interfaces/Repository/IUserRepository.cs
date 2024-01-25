using Hotels.WebAPI.Models;

namespace Hotels.WebAPI.Interfaces.Repository;

public interface IUserRepository
{
    public User? GetUser(string name);
}