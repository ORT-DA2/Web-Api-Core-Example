using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public void CreateValidUserTest()
    {
      //Arrange
      var modelIn = new UserModelIn() { Username = "Alberto", Password = "pass", Age = 2 };
      var fakeUser = new User("Alberto", "pass", 2);

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
      //Verificamos los metodos del mock
      userServiceMock.VerifyAll();

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
    private User GetFakeUser()
    {
      return new User("Test", "Test", 25);
    }
  }

}
