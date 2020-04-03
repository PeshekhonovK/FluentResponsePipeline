using System;
using System.Threading.Tasks;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IResponseHandler<out TResult, TActionResult>
    {
        /// <summary>
        /// Registers function to process data returned from previous <see cref="With{TToResult}"/> method
        /// </summary>
        /// <param name="handler">Processing method</param>
        /// <typeparam name="TToResult">New type if needed, could be same as <see cref="TResult"/></typeparam>
        /// <returns><see cref="IResponseHandler{TResult,TActionResult}"/> for further chaining with new type <see cref="TToResult"/></returns>
        IResponseHandler<TToResult, TActionResult> Process<TToResult>(Func<TResult, TToResult> handler);
        
        /// <summary>
        /// Registers function to provide new response from a source
        /// </summary>
        /// <param name="request">Returns <see cref="IResponse{TToResult}"/> from some kind of data provider</param>
        /// <typeparam name="TToResult">Type of data provided from source</typeparam>
        /// <returns><see cref="IResponseHandler{TResult,TActionResult}"/> using type of data that would return from data source</returns>
        IResponseHandler<TToResult, TActionResult> With<TToResult>(Func<TResult, Task<IResponse<TToResult>>> request);

        /// <summary>
        /// Starts recursive evaluation of all chain of <see cref="With{TToResult}"/> and <see cref="Process{TToResult}"/> methods registered before
        /// When result or error is calculated, calls <see cref="onSuccess"/> or <see cref="onError"/> parameters accordingly (if provided) or calls <see cref="IPageModelBase{TActionResult}" /> to process result generically
        /// Do not process any further registered methods if any error or exception occured
        /// </summary>
        /// <param name="page">Page or controller context of request</param>
        /// <param name="responseComposer">Helper class to create correct <see cref="IResponse{TResult}" objects/></param>
        /// <param name="onSuccess">Optional success handler</param>
        /// <param name="onError">Optional error handler</param>
        /// <typeparam name="TPage">Context object of handling the result, should be inherited from type <see cref="IPageModelBase{TActionResult}"/></typeparam>
        /// <returns></returns>
        Task<TActionResult> Evaluate<TPage>(
            TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
                where TPage : IPageModelBase<TActionResult>;
    }
}