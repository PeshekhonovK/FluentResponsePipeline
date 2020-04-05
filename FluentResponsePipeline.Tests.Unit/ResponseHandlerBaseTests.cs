using System;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using Moq;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    [TestFixture]
    public class ResponseHandlerBaseTests : TestBase
    {
        [Test]
        public void ProcessResponse_Error_Logs()
        {
            // Arrange
            var logger = GetMock<IObjectLogger>();

            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();
            
            // Act
            var result = handler.ProcessResponse(logger, response);
            
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

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ProcessResponse(logger, response);
            
            // Assert
            result.Should().Be(response);
            logger.Verify(x => x.LogTrace(response));
            logger.VerifyNever(x => x.LogError(response));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ApplyToPage_NoCustomHandlers_CallsPageReturn(bool success)
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(expected);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public void ApplyToPage_Success_CustomHandlerError_CallsPageReturn()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, object>((r, o) => new object());

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(expected);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public void ApplyToPage_Error_CustomHandlerSuccess_CallsPageReturn()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, object>((r, o) => new object());

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(expected);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page, onSuccess: successHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public void ApplyToPage_Success_CustomHandlerSuccess_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, object>((r, o) => expected);

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page, onSuccess: successHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public void ApplyToPage_Error_CustomHandlerError_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, object>((r, o) => expected);

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public void ApplyToPage_Success_CustomHandlersBoth_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, object>((r, o) => expected);
            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, object>((r, o) => new object());

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page, onSuccess: successHandler, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public void ApplyToPage_Error_CustomHandlersBoth_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, object>((r, o) => new object());
            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, object>((r, o) => expected);

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ApplyToPage(response, page, onSuccess: successHandler, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
    }
}