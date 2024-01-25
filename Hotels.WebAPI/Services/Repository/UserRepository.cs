using Hotels.WebAPI.Interfaces.Repository;
using Hotels.WebAPI.Models;

namespace Hotels.WebAPI.Services.Repository;

public class UserRepository : IUserRepository
{
    private readonly List<User> _users =
    [
        new User() { Name = "Admin", Password = "Admin" },
        new User() { Name = "Monica", Password = "123" },
        new User() { Name = "Nancy", Password = "123" }
    ];

    public User? GetUser(string name) =>
        _users.FirstOrDefault(u => u.Name == name);
}