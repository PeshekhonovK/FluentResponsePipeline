using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    public class CombineTests : TestBase
    {
        [Test]
        public async Task TwoResponses_Success_ReturnsResult()
        {
            // Arrange
            var expected = new object();
            
            const decimal payload1 = 1000m;
            const int payload2 = 5000;
            const string payload3 = "test-2";
            
            var results = new List<object>();

            var response1 = ResponseStub<decimal>.Success(payload1);
            var response2 = ResponseStub<int>.Success(payload2);
            var transformed1 = ResponseStub<string>.Success(payload3);

            var responseComposer = GetMock<IResponseComposer>();
            responseComposer.Setup(x => x.Success(payload3)).Returns(transformed1);
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(transformed1)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response1))
                .Get(r =>
                {
                    results.Add(r);
                    return Task.FromResult(response2);
                })
                .Combine((p, r) =>
                {
                    results.Add(p);
                    results.Add(r);
                    return payload3;
                })
                .EvaluateAsync(page, responseComposer);
             
            // Assert
            result.Should().Be(expected);
            results[0].Should().Be(payload1);
            results[1].Should().Be(payload1);
            results[2].Should().Be(payload2);
            results.Should().HaveCount(3);
            
            logger.Trace.Should()
                .Contain(response1)
                .And.Contain(response2)
                .And.Contain(transformed1)
                .And.HaveCount(3);
            logger.Error.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task TwoResponses_ErrorOnFirstGet_ReturnsError()
        {
            // Arrange
            var expected = new object();
            
            const int payload2 = 5000;
            const string payload3 = "test-2";
            const string message1 = "test-error-1";
            const string message2 = "test-error-2";
            
            var results = new List<object>();

            var response1 = ResponseStub<decimal>.Error(message1);
            var response2 = ResponseStub<int>.Success(payload2);
            var transformed1 = ResponseStub<string>.Success(payload3);
            
            var errorResponse2 =  ResponseStub<string>.Error(message2);

            var responseComposer = GetMock<IResponseComposer>();
            responseComposer.Setup(x => x.From<decimal, string>(response1)).Returns(errorResponse2);
            responseComposer.Setup(x => x.Success(payload3)).Returns(transformed1);
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(errorResponse2)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response1))
                .Get(r =>
                {
                    results.Add(r);
                    return Task.FromResult(response2);
                })
                .Combine((p, r) =>
                {
                    results.Add(p);
                    results.Add(r);
                    return payload3;
                })
                .EvaluateAsync(page, responseComposer);
             
            // Assert
            result.Should().Be(expected);
            results.Should().BeEmpty();
            
            logger.Trace.Should().BeEmpty();
            logger.Error.Should()
                .Contain(response1)
                .And.HaveCount(1);
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task TwoResponses_ErrorOnSecondGet_ReturnsError()
        {
            // Arrange
            var expected = new object();
            
            const decimal payload1 = 1000m;
            const string payload3 = "test-2";
            const string message1 = "test-error-1";
            const string message2 = "test-error-2";
            
            var results = new List<object>();

            var response1 = ResponseStub<decimal>.Success(payload1);
            var response2 = ResponseStub<int>.Error(message1);
            var transformed1 = ResponseStub<string>.Success(payload3);
            
            var errorResponse2 =  ResponseStub<string>.Error(message2);

            var responseComposer = GetMock<IResponseComposer>();
            responseComposer.Setup(x => x.From<string>(response2)).Returns(errorResponse2);
            responseComposer.Setup(x => x.Success(payload3)).Returns(transformed1);
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(errorResponse2)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response1))
                .Get(r =>
                {
                    results.Add(r);
                    return Task.FromResult(response2);
                })
                .Combine((p, r) =>
                {
                    results.Add(p);
                    results.Add(r);
                    return payload3;
                })
                .EvaluateAsync(page, responseComposer);
             
            // Assert
            result.Should().Be(expected);
            results[0].Should().Be(payload1);
            results.Should().HaveCount(1);
            
            logger.Trace.Should()
                .Contain(response1)
                .And.HaveCount(1);
            logger.Error.Should()
                .Contain(response2)
                .And.Contain(errorResponse2)
                .And.HaveCount(2);
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task TwoResponses_ExceptionOnCombine_ReturnsError()
        {
            // Arrange
            var expected = new object();
            
            const decimal payload1 = 1000m;
            const int payload2 = 5000;
            const string payload3 = "test-2";
            const string message = "test-error-1";

            var exception = new Exception();
            
            var results = new List<object>();

            var response1 = ResponseStub<decimal>.Success(payload1);
            var response2 = ResponseStub<int>.Success(payload2);
            var transformed1 = ResponseStub<string>.Success(payload3);
            var errorResponse =  ResponseStub<string>.Error(message);

            var responseComposer = GetMock<IResponseComposer>();
            responseComposer.Setup(x => x.Success(payload3)).Returns(transformed1);
            responseComposer.Setup(x => x.Error<string>(exception)).Returns(errorResponse);
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(errorResponse)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response1))
                .Get(r =>
                {
                    results.Add(r);
                    return Task.FromResult(response2);
                })
                .Combine<string>((p, r) =>
                {
                    results.Add(p);
                    results.Add(r);
                    throw exception;
                })
                .EvaluateAsync(page, responseComposer);
             
            // Assert
            result.Should().Be(expected);
            results[0].Should().Be(payload1);
            results[1].Should().Be(payload1);
            results[2].Should().Be(payload2);
            results.Should().HaveCount(3);
            
            logger.Trace.Should()
                .Contain(response1)
                .And.Contain(response2)
                .And.HaveCount(2);
            logger.Error.Should()
                .Contain(errorResponse)
                .And.HaveCount(1);
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
    }
}