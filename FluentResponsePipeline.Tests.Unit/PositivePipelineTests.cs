using System;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    [TestFixture]
    public class PositivePipelineTests : TestBase
    {
        [Test]
        public async Task SingleResponse_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            response.Setup(x => x.Payload).Returns(1000m);

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
            logger.Verify(x => x.LogTrace(response));
        }
        
        [Test]
        public async Task SingleResponse_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            var expected = 1000m;
            response.Setup(x => x.Payload).Returns(expected);

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(new object());

            decimal? receivedValue = null;
            
            // Act
            var result = await ResponsePipeline<object>
                .With(() => Task.FromResult(response))
                .Evaluate(page, responseComposer, ((value, p) =>
                {
                    receivedValue = value;
                    return p;
                }), null);
             
            // Assert
            result.Should().Be(page);
            receivedValue.Should().Be(expected);
            logger.Verify(x => x.LogTrace(response));
        }
    }
}