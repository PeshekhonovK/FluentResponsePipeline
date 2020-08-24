using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Internal;
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
            var logger = new LoggerStub();

            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();
            
            // Act
            var result = handler.ProcessResponse(logger, response);
            
            // Assert
            result.Should().Be(response);
            
            logger.Trace.Should().BeEmpty();
            logger.Error.Should().Contain(response).And.HaveCount(1);
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public void ProcessResponse_Success_LogsTrace()
        {
            // Arrange
            var logger = new LoggerStub();

            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = handler.ProcessResponse(logger, response);
            
            // Assert
            result.Should().Be(response);
            
            logger.Trace.Should().Contain(response).And.HaveCount(1);
            logger.Error.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        [Test]
        public void ProcessResponse_EmptyResponse_LogsNothing()
        {
            // Arrange
            var logger = new LoggerStub();

            var response = new EmptyResponse<object>();

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();
            
            // Act
            var result = handler.ProcessResponse(logger, response);
            
            // Assert
            result.Should().Be(response);
            
            logger.Trace.Should().BeEmpty();
            logger.Error.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public async Task ApplyToPage_NoCustomHandlers_CallsPageReturn(bool success)
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(expected);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task ApplyToPage_Success_CustomHandlerError_CallsPageReturn()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(new object()));

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(expected);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task ApplyToPage_Error_CustomHandlerSuccess_CallsPageReturn()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(new object()));

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(expected);

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page, onSuccess: successHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task ApplyToPage_Success_CustomHandlerSuccess_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(expected));

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page, onSuccess: successHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task ApplyToPage_Error_CustomHandlerError_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(expected));

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task ApplyToPage_Success_CustomHandlersBoth_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(expected));
            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(new object()));

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page, onSuccess: successHandler, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task ApplyToPage_Error_CustomHandlersBoth_CallsHandler()
        {
            // Arrange
            var response = GetMock<IResponse<object>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var expected = new object();

            var successHandler = new Func<object, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(new object()));
            var errorHandler = new Func<IResponse<object>, IPageModelBase<object>, Task<object>>((r, o) => Task.FromResult(expected));

            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Return(response)).Returns(new object());

            var handler = GetPartialMock<ResponseHandlerBase<object, object>>();

            // Act
            var result = await handler.ApplyToPage(response, page, onSuccess: successHandler, onError: errorHandler);
            
            // Assert
            result.Should().Be(expected);
        }
    }
}