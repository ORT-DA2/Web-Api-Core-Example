using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using WAC.WebAPI.Controllers;
using WAC.WebAPI.Models;
using WAC.Domain.Users;
using Microsoft.AspNetCore.Mvc;
using WAC.Contracts.Application.Users;
using Moq;

namespace WAC.WebAPI.Tests
{
  [TestClass]
  public class UserCreationTest
  {

    [TestMethod]
    public void GetAllUsersOkTest()
    {
      //Arrange: Construimos el mock y seteamos las expectativas
      var expectedUsers = GetFakeUsers();
      var mockUserService = new Mock<IUserService>();
      mockUserService
          .Setup(bl => bl.GetAll())
          .Returns(expectedUsers);
      var controller = new UsersController(mockUserService.Object);

      //Act
      var obtainedResult = controller.Get() as ActionResult<List<User>>;

      //Assert
      mockUserService.VerifyAll();
      Assert.IsNotNull(obtainedResult);
      Assert.IsNotNull(obtainedResult.Value);
      Assert.AreEqual(obtainedResult.Value, expectedUsers);
    }


    [TestMethod]
    public void GetUserOkTest()
    {
      //Arrange: Construimos el mock y seteamos las expectativas
      var expectedUser = GetFakeUser();
      var mockUserService = new Mock<IUserService>();
      mockUserService
          .Setup(bl => bl.Get(expectedUser.Id))
          .Returns(expectedUser);

      var controller = new UsersController(mockUserService.Object);

      //Act
      var obtainedResult = controller.Get(expectedUser.Id) as ActionResult<User>;

      //Assert
      mockUserService.Verify(m => m.Get(expectedUser.Id), Times.AtMostOnce());
      Assert.IsNotNull(obtainedResult);
      Assert.IsNotNull(obtainedResult.Value);
      Assert.AreEqual(obtainedResult.Value, expectedUser);
    }

    [TestMethod]
    public void CreateValidUserTest()
    {
      //Arrange
      var modelIn = new UserModelIn() { Username = "Alberto", Password = "pass", Age = 2 };
      var fakeUser = GetFakeUser();

      // Inicializar el mock a partir de IUserService
      var userServiceMock = new Mock<IUserService>();
      // Esperamos que se llame el motodo SignUp del servicio
      userServiceMock.Setup(userService => userService.SignUp(fakeUser));
      var controller = new UsersController(userServiceMock.Object);

      //Act
      var result = controller.Post(modelIn);
      var createdResult = result as CreatedAtRouteResult;
      var modelOut = createdResult.Value as UserModelOut;

      //Assert
      Assert.IsNotNull(createdResult);
      Assert.AreEqual("GetById", createdResult.RouteName);
      Assert.AreEqual(201, createdResult.StatusCode);
      Assert.AreEqual(modelIn.Username, modelOut.Username);
      Assert.AreEqual(modelIn.Age, modelOut.Age);
    }

    [TestMethod]
    public void CreateFailedUserTest()
    {
      //Arrange
      var modelIn = new UserModelIn();
      var userService = new Mock<IUserService>();
      var controller = new UsersController(userService.Object);
      //We need to force the error in de ModelState
      controller.ModelState.AddModelError("", "Error");
      var result = controller.Post(modelIn);

      //Act
      var createdResult = result as BadRequestObjectResult;

      //Assert
      Assert.IsNotNull(createdResult);
      Assert.AreEqual(400, createdResult.StatusCode);
    }
    private List<User> GetFakeUsers()
    {
      return new List<User>{
        new User("John", "Doe", 25),
        new User("Pete", "Doe", 21),
      };
    }

    private User GetFakeUser()
    {
      return new User("John", "Doe", 25);
    }
  }

}
