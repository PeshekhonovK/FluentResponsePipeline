using System;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using Moq;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    public class ResponseHandlerBaseTests : TestBase
    {
        [Test]
        public void ProcessResponse_Error_Logs()
        {
            // Arrange
            var logger = GetMock<IObjectLogger>();

            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            // Act
            var result = ResponseHandlerBase<object, object>.ProcessResponse(logger, response);
            
            // Assert
            result.Should().Be(response);
            logger.Verify(x => x.LogError(response));
            logger.VerifyNever(x => x.LogTrace(response));
        }
        
        [Test]
        public void ProcessResponse_Success_LogsTrace()
        {
            // Arrange
            var logger = GetMock<IObjectLogger>();

            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            // Act
            var result = ResponseHandlerBase<object, object>.ProcessResponse(logger, response);
            
            // Assert
            result.Should().Be(response);
            logger.Verify(x => x.LogTrace(response));
            logger.VerifyNever(x => x.LogError(response));
        }
    }
}