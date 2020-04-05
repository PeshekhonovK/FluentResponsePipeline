using System;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    [TestFixture]
    public class NegativePipelineTests : TestBase
    {
        [Test]
        public async Task SingleResponse_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .With(() => Task.FromResult(response))
                .Evaluate(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            logger.Verify(x => x.LogError(response));
        }
        
        [Test]
        public async Task SingleResponse_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(new object());

            IResponse<decimal> receivedResponse = null;
            
            // Act
            var result = await ResponsePipeline<object>
                .With(() => Task.FromResult(response))
                .Evaluate(page, responseComposer, null, ((r, p) =>
                {
                    receivedResponse = r;
                    return p;
                }));
             
            // Assert
            result.Should().Be(page);
            receivedResponse.Should().Be(response);
            logger.Verify(x => x.LogError(response));
        }
    }
}