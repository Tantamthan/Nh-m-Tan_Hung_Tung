using ASCwed.Cofiguration;
using ASCwed.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace ASC.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsView_WithCorrectModel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HomeController>>();

            var mockOptions = new Mock<IOptions<ApplicationSettings>>();
            mockOptions.Setup(x => x.Value).Returns(new ApplicationSettings
            {
                ApplicationName = "Test App"
            });

            var controller = new HomeController(mockLogger.Object, mockOptions.Object);

            // Fake HttpContext + Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(viewResult.Model);

            // ✅ ĐÚNG: model là string
            var model = Assert.IsType<string>(viewResult.Model);

            Assert.Equal("Test App", model);
        }
    }
}