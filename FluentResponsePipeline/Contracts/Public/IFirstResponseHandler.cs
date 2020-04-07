using System;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IFirstResponseHandler<out TRequestResult, out TResult, TActionResult> : IBaseResponseHandler<TResult, TActionResult>
    {
        /// <summary>
        /// Registers method to transform <typeparamref name="TResult"/> to new <typeparamref name="TTransformResult"/>
        /// </summary>
        /// <param name="transform">Transformation logic from <typeparamref name="TResult"/> and <typeparamref name="TParentResult"/> to <typeparamref name="TTransformResult"/></param>
        /// <typeparam name="TTransformResult">New result type</typeparam>
        /// <returns>New response handler without possibility to add new transform, but replace existing one</returns>
        IFirstResponseHandlerWithTransform<TRequestResult, TTransformResult, TActionResult> Transform<TTransformResult>(Func<IResponse<TRequestResult>, IResponse<TTransformResult>> transform);
    }
}