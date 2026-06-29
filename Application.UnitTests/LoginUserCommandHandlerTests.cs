using Application.Abstractions.Authentication;
using Application.Ports;
using Application.Users.Login;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests
{
    public class LoginUserCommandHandlerTests
    {

        private readonly static LoginUserCommand command = new
        (
            Email: "test@test.com",
            Password: "12345678_"
        );

        private readonly IUserRepository _userRepositoryMock;
        private readonly IPasswordHasher _passwordHasherMock;
        private readonly ITokenProvider _tokenProviderMock;
        private readonly IUnitOfWork _unitOfWorkMock;

        private readonly LoginUserCommandHandler _handler;


        public LoginUserCommandHandlerTests()
        {
            _userRepositoryMock = Substitute.For<IUserRepository>();
            _passwordHasherMock = Substitute.For<IPasswordHasher>();
            _tokenProviderMock = Substitute.For<ITokenProvider>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _handler = new LoginUserCommandHandler(_userRepositoryMock, _passwordHasherMock, _tokenProviderMock, _unitOfWorkMock);
        }

    }
}
