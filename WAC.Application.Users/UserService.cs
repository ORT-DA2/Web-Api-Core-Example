using System;
using System.Collections.Generic;
using WAC.Contracts.Application.Users;
using WAC.Domain.Users;
using System.Linq;

namespace WAC.Application.Users
{
  public class UserService : IUserService
  {
    private static ICollection<User> Users = new List<User>();

    public void SignUp(User user)
    {
      TrySignUp(user);
    }
    protected virtual void TrySignUp(User user)
    {
      Users.Add(user);
    }
    public User Get(int userId)
    {
      return TryGet(userId);
    }
    protected virtual User TryGet(int userId)
    {
      return Users.First(u => u.Id == userId);
    }
  }
}
