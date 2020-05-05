using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResponsePipeline.Contracts.Public;
using NUnit.Framework;

namespace FluentResponsePipeline.Tests.Unit
{
    [TestFixture]
    public class DuplicateMethodCallsNegativeTests : TestBase
    {
        [Test]
        public void FirstResponsePipeline_DuplicateTransform_ThrowsError()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            
            var withOneTransform = (IFirstResponseHandler<decimal, decimal, object>)ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Transform(Task.FromResult);
            
            // Act
            var action = new Action(() => withOneTransform.Transform(Task.FromResult));

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("This get already has a transform");
        }
        
        [Test]
        public void FirstResponsePipeline_TransformAfterTry_ThrowsError()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            var tryResponse = GetMock<IResponse>();
                
            var withOneTransform = (IResponseHandler<decimal, decimal, decimal, object>)ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Try(x => Task.FromResult(tryResponse));
            
            // Act
            var action = new Action(() => withOneTransform.Transform((_, r) => Task.FromResult(r)));

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("This get already has a try logic");
        }
        
        [Test]
        public void FirstResponsePipeline_DuplicateTransformAfterSecondGet_ThrowsError()
        {
            // Arrange
            var response = GetMock<IResponse<decimal>>();
            
            var withOneTransform = (IResponseHandler<decimal, decimal, decimal, object>)ResponsePipeline<object>
                .Get(() => Task.FromResult(response))
                .Get((r) => Task.FromResult(response))
                .Transform((_, r) => Task.FromResult(r));
            
            // Act
            var action = new Action(() => withOneTransform.Transform((_, r) => Task.FromResult(r)));

            // Assert
            action.Should().Throw<InvalidOperationException>().WithMessage("This get already has a transform");
        }
    }
}