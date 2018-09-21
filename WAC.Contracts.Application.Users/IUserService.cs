using System;
using WAC.Domain.Users;
using System.Collections.Generic;

namespace WAC.Contracts.Application.Users
{
  public interface IUserService
  {
    void SignUp(User user);
    User Get(int userId);
    List<User> GetAll();
  }
}
