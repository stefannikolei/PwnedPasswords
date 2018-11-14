using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Options;
using Moq;
using PwnedPasswords.Client;
using Xunit;

namespace PwnedPasswords.Validator.Test
{
    public class PwnedPasswordValidatorTests
    {
        const string _error = "The password you chose has appeared in a data breach.";
        readonly IOptions<PwnedPasswordValidatorOptions> Settings = 
            Options.Create(new PwnedPasswordValidatorOptions { ErrorMessage = _error });

        [Fact]
        public void ValidateThrowsWithNullClientTest()
        {
            // Setup
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>("client", () => new PwnedPasswordValidator<TestUser>(null, Settings));
        }

        [Fact]
        public async Task SucceedsIfNullPassword()
        {
            var client = new Mock<IPwnedPasswordsClient>();

            string input = null;
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(client.Object, Settings);

            IdentityResultAssert.IsSuccess(await validator.ValidateAsync(manager, null, input));
        }

        [Fact]
        public async Task SucceedsIfZeroLengthPassword()
        {
            var client = new Mock<IPwnedPasswordsClient>();

            var input = string.Empty;
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(client.Object, Settings);

            IdentityResultAssert.IsSuccess(await validator.ValidateAsync(manager, null, input));
        }

        [Fact]
        public async Task FailsIfclientIndicatesPasswordIsPwned()
        {
            var client = new Mock<IPwnedPasswordsClient>();
            client.Setup(x => x.HasPasswordBeenPwned(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var input = "password";
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(client.Object, Settings);

            IdentityResultAssert.IsFailure(await validator.ValidateAsync(manager, null, input), _error);
        }
        
        [Fact]
        public async Task SuccessIfclientIndicatesPasswordIsNotPwned()
        {
            var client = new Mock<IPwnedPasswordsClient>();
            client.Setup(x => x.HasPasswordBeenPwned(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var input = "password";
            var manager = MockHelpers.TestUserManager<TestUser>();
            var validator = new PwnedPasswordValidator<TestUser>(client.Object, Settings);

            IdentityResultAssert.IsSuccess(await validator.ValidateAsync(manager, null, input));
        }
        
    }
}