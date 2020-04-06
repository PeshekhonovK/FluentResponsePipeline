using System;
using System.Threading.Tasks;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IResponseHandlerWithTransform<out TParentResult, out TRequestResult, out TResult, TActionResult> 
        : IBaseResponseHandler<TResult, TActionResult>
    {
        /// <summary>
        /// Replaces transform registered for current handler
        /// Registers method to transform <typeparamref name="TResult"/> to new <typeparamref name="TTransformResult"/>.
        /// Allows usage of source <typeparamref name="TParentResult"/> object
        /// </summary>
        /// <param name="transform">Transformation logic from <typeparamref name="TResult"/> and <typeparamref name="TParentResult"/> to <typeparamref name="TTransformResult"/></param>
        /// <typeparam name="TTransformResult">New result type</typeparam>
        /// <returns>New response handler without possibility to add new transform, but replace existing one</returns>
        IResponseHandlerWithTransform<TParentResult, TRequestResult, TTransformResult, TActionResult> ReplaceTransform<TTransformResult>(Func<IResponse<TParentResult>, IResponse<TRequestResult>, IResponse<TTransformResult>> transform);
    }
}