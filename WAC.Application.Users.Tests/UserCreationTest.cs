using Microsoft.VisualStudio.TestTools.UnitTesting;
using WAC.Contracts.Application.Users;
using WAC.Application.Users;
using WAC.Domain.Users;
using WAC.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace WAC.Application.Users.Tests
{
  [TestClass]
  public class UserCreationTest
  {
    private IUserService userService;
    private User user = new User("User1", "Pass", 23);

    [TestInitialize]
    public void SetUp()
    {
      var options = new DbContextOptionsBuilder<DomainContext>()
        .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
        .Options;

      userService = new UserService(new DomainContext(options));
    }


    [TestMethod]
    public void SignUpTest()
    {
      userService.SignUp(user);
      Assert.AreEqual(user, userService.Get(user.Id));
    }

  }
}
