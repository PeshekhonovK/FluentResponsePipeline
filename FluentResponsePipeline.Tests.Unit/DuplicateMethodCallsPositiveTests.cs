using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    [TestFixture]
    public class DuplicateMethodCallsPositiveTests : TestBase
    {
        [Test]
        public async Task SingleResponse_TryAfterTry_ReturnsResult()
        {
            // Arrange
            const decimal payload = 1000m;
            
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            response.Setup(x => x.Payload).Returns(payload);

            var results = new List<object>();

            var expected = new object();

            var responseComposer = GetMock<IResponseComposer>();
            
            var positiveTry = ResponseStub.Success();
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(response)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Try(r =>
                {
                    results.Add(r);
                    return Task.FromResult(positiveTry);
                })
                .Try(r =>
                {
                    results.Add(r);
                    return Task.FromResult(positiveTry);
                })
                .Evaluate(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            results.Should().OnlyContain(x => x is decimal && (decimal)x == payload).And.HaveCount(2);
            
            logger.Trace.Should()
                .Contain(response)
                .And.Contain(positiveTry)
                .And.HaveCount(3);
            logger.Error.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
        
        [Test]
        public async Task SingleResponse_TryAfterTransform_ReturnsResult()
        {
            // Arrange
            const decimal payload = 1000m;
            const string transformed = "test";
            
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Succeeded).Returns(true);
            response.Setup(x => x.Payload).Returns(payload);

            var results = new List<object>();

            var expected = new object();

            var responseComposer = GetMock<IResponseComposer>();
            
            var positiveTry = ResponseStub.Success();
            var transformedResponse = ResponseStub<string>.Success(transformed);
            
            var logger = new LoggerStub();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);
            page.Setup(x => x.Return(transformedResponse)).Returns(expected);
            
            // Act
            var result = await ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Transform(r =>
                {
                    results.Add(r.Payload);
                    return Task.FromResult(transformedResponse);
                })
                .Try(r =>
                {
                    results.Add(r);
                    return Task.FromResult(positiveTry);
                })
                .Evaluate(page, responseComposer, null, null);
             
            // Assert
            result.Should().Be(expected);
            results.Should().Contain(payload).And.Contain(transformed).And.HaveCount(2);
            
            logger.Trace.Should()
                .Contain(response)
                .And.Contain(positiveTry)
                .And.Contain(transformedResponse)
                .And.HaveCount(3);
            logger.Error.Should().BeEmpty();
            logger.Debug.Should().BeEmpty();
            logger.Information.Should().BeEmpty();
            logger.Warning.Should().BeEmpty();
            logger.Critical.Should().BeEmpty();
        }
    }
}