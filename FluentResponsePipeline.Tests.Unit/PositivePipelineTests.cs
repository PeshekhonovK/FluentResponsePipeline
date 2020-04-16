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
                .Get(() => Task.FromResult(response))
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
            const decimal expected = 1000m;
            response.Setup(x => x.Payload).Returns(expected);

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(new object());

            decimal? receivedValue = null;
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
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
        
        [Test]
        public async Task SingleResponse_WithTransform_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);

            var expected = new object();
            
            const string expectedPayload = "test";
            var transformed = (IResponse<string>)new ResponseStub<string>() { Succeeded = true, Payload = expectedPayload };

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(transformed)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Transform(r => Task.FromResult(transformed))
                .Evaluate(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            logger.Verify(x => x.LogTrace(It.Is<IResponse<string>>(r => r.Payload == expectedPayload)));
        }
        
        [Test]
        public async Task SingleResponse_WithTransform_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            const string expected = "expected";

            var responseComposer = GetMock<IResponseComposer>();
            var transformed = (IResponse<string>)new ResponseStub<string>() { Succeeded = true, Payload = expected };
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(transformed)).Returns(new object());

            string receivedValue = null;
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Transform(r => Task.FromResult(transformed))
                .Evaluate(page, responseComposer, ((value, p) =>
                {
                    receivedValue = value;
                    return p;
                }), null);
             
            // Assert
            result.Should().Be(page);
            receivedValue.Should().Be(expected);
            logger.Verify(x => x.LogTrace(It.Is<IResponse<string>>(r => r.Payload == expected)));
        }
        
        [Test]
        public async Task TwoResponses_ReturnsResult()
        {
            // Arrange
            var results = new List<object>();
            var response = ResponseStub<object>.Success(new object());

            var expected = new object();

            var responseComposer = GetMock<IResponseComposer>();

            const decimal payload1 = 1000m;
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(ResponseStub<decimal>.Success(payload1)))
                .Get(t =>
                {
                    results.Add(t);
                    return Task.FromResult(response);
                })
                .Evaluate(page, responseComposer, null, null);
             
            // Assert
            results.Should().Contain(payload1).And.HaveCount(1);
            result.Should().Be(expected);
            logger.Verify(x => x.LogTrace(response));
        }
        
        [Test]
        public async Task TwoResponses_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            const decimal payload1 = 1000m;
            const string payload2 = "test";
            
            var results = new List<object>();
            
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            response.Setup(x => x.Payload).Returns(payload1);

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(new object());
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Get(t =>
                {
                    results.Add(t);
                    return Task.FromResult(ResponseStub<string>.Success(payload2));
                })
                .Evaluate(page, responseComposer, ((value, p) =>
                {
                    results.Add(value);
                    return p;
                }), null);
             
            // Assert
            result.Should().Be(page);
            results.Should().Contain(payload1).And.Contain(payload2).And.HaveCount(2);
            logger.Verify(x => x.LogTrace(response));
        }
        
        [Test]
        public async Task TwoResponses_WithTransform_ReturnsResult()
        {
            // Arrange
            var expected = new object();
            
            const string payload1 = "test";
            const int payload2 = 5000;
            const bool payload3 = true;
            
            var results = new List<object>();

            var response1 = ResponseStub<decimal>.Success(1000m);
            var response2 = ResponseStub<int>.Success(payload2);
            var transformed1 = ResponseStub<string>.Success(payload1);
            var transformed2 = ResponseStub<bool>.Success(payload3);

            var responseComposer = GetMock<IResponseComposer>();
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(transformed2)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response1))
                .Transform(r =>
                {
                    results.Add(r);
                    return Task.FromResult(transformed1);
                })
                .Get(r =>
                {
                    results.Add(r);
                    return Task.FromResult(response2);
                })
                .Transform((p, r) =>
                {
                    results.Add(p);
                    results.Add(r);
                    return Task.FromResult(transformed2);
                })
                .Evaluate(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            results[0].Should().Be(response1);
            results[1].Should().Be(payload1);
            results[2].Should().BeAssignableTo<IResponse<string>>().And.Match(x => (x as IResponse<string>).Payload == payload1);
            results[3].Should().BeAssignableTo<IResponse<int>>().And.Match(x => (x as IResponse<int>).Payload == payload2);
            results.Should().HaveCount(4);
            
            logger.Verify(x => x.LogTrace(response1));
            logger.Verify(x => x.LogTrace(response2));
            logger.Verify(x => x.LogTrace(It.Is<IResponse<string>>(r => r.Payload == payload1)));
            logger.Verify(x => x.LogTrace(It.Is<IResponse<bool>>(r => r.Payload == payload3)));
        }
        
        [Test]
        public async Task TwoResponses_WithTransform_WithCustomHandler_ReturnsResult()
        {
            // Arrange
            const string payload2 = "test";
            const bool payload3 = true;
            
            var results = new List<object>();
            
            const string payload1 = "expected";

            const decimal payload0 = 1000m;

            var responseComposer = GetMock<IResponseComposer>();
            
            var response1 = ResponseStub<decimal>.Success(payload0);
            var response2 = ResponseStub<string>.Success(payload2);
            var transformed1 = ResponseStub<string>.Success(payload1);
            var transformed2 = ResponseStub<bool>.Success(payload3);
            
            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);

            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response1))
                .Transform(r =>
                {
                    results.Add(r);
                    return Task.FromResult(transformed1);
                })
                .Get(r =>
                {
                    results.Add(r);
                    return Task.FromResult(response2);
                })
                .Transform((p, r) =>
                {
                    results.Add(p);
                    results.Add(r);
                    return Task.FromResult(transformed2);
                })
                .Evaluate(page, responseComposer, ((value, p) =>
                {
                    results.Add(value);
                    return p;
                }), null);
             
            // Assert
            result.Should().Be(page);
            
            results[0].Should().Be(response1);
            results[1].Should().Be(payload1);
            results[2].Should().BeAssignableTo<IResponse<string>>().And.Match(x => (x as IResponse<string>).Payload == payload1);
            results[3].Should().BeAssignableTo<IResponse<string>>().And.Match(x => (x as IResponse<string>).Payload == payload2);
            results[4].Should().Be(payload3);
            results.Should().HaveCount(5);
            
            logger.Verify(x => x.LogTrace(response1));
            logger.Verify(x => x.LogTrace(response2));
            logger.Verify(x => x.LogTrace(It.Is<IResponse<string>>(r => r.Payload == payload1)));
            logger.Verify(x => x.LogTrace(It.Is<IResponse<bool>>(r => r.Payload == payload3)));
        }
    }
}