using System;

namespace WAC.Domain.Users
{
  public class User
  {
    public int Id { get; private set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public int Age { get; set; }

    protected User()
    {
    }

    public User(string username, string password, int age)
    {
      Username = username;
      Password = password;
      Age = age;
    }
  }
}
