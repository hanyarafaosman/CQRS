using Waseet.CQRS.Sample.Models;

namespace Waseet.CQRS.Sample.Data;

/// <summary>
/// Simple in-memory repository for demo purposes.
/// </summary>
public class UserRepository
{
    private readonly List<User> _users = new();

    public void Add(User user)
    {
        _users.Add(user);
    }

    public void Update(User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
        }
    }

    public User? GetById(Guid id)
    {
        return _users.FirstOrDefault(u => u.Id == id);
    }

    public void Delete(Guid id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _users.Remove(user);
        }
    }

    public List<User> GetAll()
    {
        return _users.ToList();
    }
}
