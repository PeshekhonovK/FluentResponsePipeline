using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using Moq;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    [TestFixture]
    public class FirstResponseProviderTests : TestBase
    {
        [Test]
        public async Task GetResult_NoError_CallsBase()
        {
            // Arrange
            var logger = GetMock<IObjectLogger>();
            var responseComposer = GetMock<IResponseComposer>();
            var response = GetMock<IResponse<object>>();
            
            var expected = GetMock<IResponse<object>>();
            
            var provider = GetPartialMock<FirstResponseProvider<object, object, object>>(new Func<Task<IResponse<object>>>(() => Task.FromResult(response)), new Func<IResponse<object>, IResponseComposer, IObjectLogger, Task<IResponse<object>>>((r, _, __) => Task.FromResult(r)));
            provider.Setup(x => x.ProcessResponse(logger, response)).Returns(expected);
            
            // Act
            var result = await provider.GetResult(logger, responseComposer);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task GetResult_Error_Logs()
        {
            // Arrange
            var exception = new Exception();
            var expected = GetMock<IResponse<object>>();
            
            var logger = GetMock<IObjectLogger>();
            var responseComposer = GetMock<IResponseComposer>();
            responseComposer.Setup(x => x.Error<object>(exception)).Returns(expected);
            
            var provider = GetPartialMock<FirstResponseProvider<object, object, object>>(new Func<Task<IResponse<object>>>(() => throw exception), new Func<IResponse<object>, IResponseComposer, IObjectLogger, Task<IResponse<object>>>((r, _, __) => Task.FromResult(r)));
            
            // Act
            var result = await provider.GetResult(logger, responseComposer);
            
            // Assert
            result.Should().Be(expected);
        }
        
        [Test]
        public async Task Evaluate_AppliesResult()
        {
            // Arrange
            const decimal value = 1000m;
            var response = GetMock<IResponse<decimal>>();
            response.Setup(x => x.Payload).Returns(value);
            var request = new Func<Task<IResponse<decimal>>>(() => Task.FromResult(response));

            var responseComposer = GetMock<IResponseComposer>();

            var logger = GetMock<IObjectLogger>();
            var page = GetMock<IPageModelBase<object>>();
            page.Setup(x => x.Logger).Returns(logger);

            var expected = new object();
            var expectedResult = GetMock<IResponse<decimal>>();
            
            var provider = GetPartialMock<FirstResponseProvider<decimal, decimal, object>>(request, new Func<IResponse<decimal>, IResponseComposer, IObjectLogger, Task<IResponse<decimal>>>((r, _, __) => Task.FromResult(r)));
            provider
                .Setup(x => x.GetResult(logger, responseComposer))
                .ReturnsAsync(expectedResult);
            provider
                .Setup(x => x.ApplyToPage(expectedResult, page, null, null))
                .Returns(expected);
            
            // Act
            var result = await provider.Evaluate(page, responseComposer, null, null);
            
            // Assert
            result.Should().Be(expected);
        }
    }
}