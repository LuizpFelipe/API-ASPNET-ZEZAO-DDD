using Application.Request;
using Application.Services;
using Domain.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests;

public class UserServiceTest
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ISecurityService _securityService = Substitute.For<ISecurityService>();
    private readonly UserService _service;

    public UserServiceTest()
    {
        _service = new UserService(_userRepository, _securityService);
    }

    [Fact]
    public async Task Should_Create_New_User_Correctly()
    {
        //Arrange
        var request = new UserRequestDTO
        {
            Name = "Joao Victor",
            Email = "joao@gmail.com",
            Password = "joao123@#$"
        };

        var passwordHashMock = "HIE2233";
        _securityService.HashPassword(request.Password).Returns(passwordHashMock);

        //Act
        var result = await _service.CreateUser(request);

        //Assert
        Assert.Equal("User created successfully", result.Message);
        Assert.Equal("Success", result.Status);
        Assert.Equal(request.Name, result.Data?.Name);
        Assert.Equal(request.Email, result.Data?.Email);
    }

    [Fact]
    public async Task Should_Return_Invalid_Argument_When_Request_Is_Null()
    {
        //Arrange
        UserRequestDTO? request = null;

        //Act
        var result = await _service.CreateUser(request!);

        //Assert
        Assert.Equal("Parameters is empty or null", result.Message);
        Assert.Equal("invalid_argument", result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CreateUser_WhenRepositoryThrowsException_ReturnsErrorResponse()
    {
        //Arrange
        var request = new UserRequestDTO
        {
            Name = "Joao Victor",
            Email = "joao@gmail.com",
            Password = "joao123@#$"
        };

        _securityService.HashPassword(Arg.Any<string>())
                       .Returns("hash123");

        _userRepository.AddAsync(Arg.Any<Domain.Entities.User>())
                       .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _service.CreateUser(request);

        // Assert
        Assert.Equal("error", result.Status);
        Assert.Null(result.Data);
    }
}
