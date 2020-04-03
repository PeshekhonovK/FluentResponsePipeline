using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace FluentResponsePipeline.Tests.Unit
{
    public static class MoqExtensions
    {
        public static ISetup<TObject, TResult> Setup<TObject, TResult>(this TObject target, Expression<Func<TObject, TResult>> expression) 
            where TObject : class
        {
            var mock = Mock.Get(target);

            return mock.Setup(expression);
        }

        public static void Verify<TObject>(this TObject target, Expression<Action<TObject>> expression) 
            where TObject : class
        {
            var mock = Mock.Get(target);

            mock.Verify(expression);
        }

        public static void VerifyNever<TObject>(this TObject target, Expression<Action<TObject>> expression) 
            where TObject : class
        {
            var mock = Mock.Get(target);

            mock.Verify(expression, Times.Never);
        }
    }
}