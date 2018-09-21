using System;
using System.Collections.Generic;
using WAC.Contracts.Application.Users;
using WAC.Domain.Users;
using WAC.DataAccess;
using System.Linq;

namespace WAC.Application.Users
{
  public class UserService : IUserService
  {
    private DomainContext _context;

    public UserService(DomainContext context)
    {
      _context = context;
    }


    public void SignUp(User user)
    {
      TrySignUp(user);
    }
    protected virtual void TrySignUp(User user)
    {
      _context.Users.Add(user);
      _context.SaveChanges();
    }
    public User Get(int userId)
    {
      return TryGet(userId);
    }

    public List<User> GetAll()
    {
      return _context.Users.ToList();
    }
    protected virtual User TryGet(int userId)
    {
      return _context.Users.First(u => u.Id == userId);
    }
  }
}
