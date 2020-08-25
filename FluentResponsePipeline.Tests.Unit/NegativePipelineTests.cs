using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using Moq;
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
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .EvaluateAsync(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            
            logger.Error.Should().Contain(response).And.HaveCount(1);
            logger.Trace.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task SingleResponse_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(false);

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(new object());

            IResponse<decimal> receivedResponse = null;
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Evaluate(page, responseComposer, null, ((r, p) =>
                {
                    receivedResponse = r;
                    return p;
                }));
             
            // Assert
            result.Should().Be(page);
            receivedResponse.Should().Be(response);
            
            logger.Error.Should().Contain(response).And.HaveCount(1);
            logger.Trace.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task SingleResponse_WithTransform_ReturnsResult()
        {
            // Arrange
            const string expectedError = "error";
            var response = ResponseStub<decimal>.Error(expectedError);

            const string expectedPayload = "test";
            var transformed = new ResponseStub<string> { Succeeded = true, Payload = expectedPayload };
            
            var responseComposer = GetMock<IResponseComposer>();

            var expected = new object();
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(transformed)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Transform(r => Task.FromResult((IResponse<string>)transformed))
                .EvaluateAsync(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            
            logger.Error.Should().Contain(x => x is IResponse<decimal> && !((IResponse<decimal>)x).Succeeded && ((IResponse<decimal>)x).Message == expectedError).And.HaveCount(1);
            logger.Trace.Should().Contain(transformed).And.HaveCount(1);
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task SingleResponse_WithTransform_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            const string expectedError = "error";
            var response = ResponseStub<decimal>.Error(expectedError);

            var responseComposer = GetMock<IResponseComposer>();
            
            const string expected = "expected";
            var transformed = new ResponseStub<string>() { Succeeded = true, Payload = expected };
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(new object());

            string receivedValue = null;
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Transform(r => Task.FromResult((IResponse<string>)transformed))
                .Evaluate(page, responseComposer, ((value, p) =>
                {
                    receivedValue = value;
                    return p;
                }), null);
             
            // Assert
            result.Should().Be(page);
            receivedValue.Should().Be(expected);
            
            logger.Error.Should().Contain(x => x is IResponse<decimal> && !((IResponse<decimal>)x).Succeeded && ((IResponse<decimal>)x).Message == expectedError).And.HaveCount(1);
            logger.Trace.Should().Contain(transformed).And.HaveCount(1);
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task SingleResponse_WithFailedTry_ReturnsResult()
        {
            // Arrange
            const decimal payload = 1000m;
            
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            response.Setup(x => x.Payload).Returns(payload);

            var results = new List<object>();

            var expected = new object();

            const string expectedError = "E";
            var negativeTry = ResponseStub.Error(expectedError);

            var errorResult = ResponseStub<decimal>.Error("E2");
            
            var responseComposer = GetMock<IResponseComposer>();
            responseComposer
                .Setup(x => x.From<decimal>(negativeTry))
                .Returns(errorResult);
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(errorResult)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Try(r =>
                {
                    results.Add(r);
                    return Task.FromResult(negativeTry);
                })
                .EvaluateAsync(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            results.Should().Contain(payload).And.HaveCount(1);
            logger.Trace.Should().Contain(response).And.HaveCount(1);
            logger.Error.Should().Contain(negativeTry)
                .And.HaveCount(1);
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
    }
}