using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IBaseResponseHandler<TResult, TActionResult>
    {
        /// <summary>
        /// Registers function to provide new response from a source
        /// </summary>
        /// <param name="request">Returns <see cref="IResponse{TToResult}"/> from some kind of data provider</param>
        /// <typeparam name="TToResult">Type of data provided from source</typeparam>
        IResponseHandler<TResult, TToResult, TToResult, TActionResult> Get<TToResult>(Func<TResult, Task<IResponse<TToResult>>> request);
        
        ITryResponseHandler<TResult, TActionResult> Try(Func<TResult, Task<IResponse>> request);
        
        /// <summary>
        /// Starts recursive evaluation of all chain of <see cref="Get{TToResult}"/> and <see cref="Process"/> methods registered before
        /// When result or error is calculated, calls <see cref="onSuccess"/> or <see cref="onError"/> parameters accordingly (if provided) or calls <see cref="IPageModelBase{TActionResult}" /> to process result generically
        /// Do not process any further registered methods if any error or exception occured
        /// </summary>
        /// <param name="page">Page or controller context of request</param>
        /// <param name="responseComposer">Helper class to create correct <see cref="IResponse{TResult}"/> objects</param>
        /// <param name="onSuccess">Optional success handler</param>
        /// <param name="onError">Optional error handler</param>
        /// <returns></returns>
        Task<TActionResult> Evaluate<TPage>(
            TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
            where TPage : IPageModelBase<TActionResult>;

        /// <summary>
        /// Starts recursive evaluation of all chain of <see cref="Get{TToResult}"/> and <see cref="Process"/> methods registered before
        /// When result or error is calculated, calls <see cref="onSuccess"/> or <see cref="onError"/> parameters accordingly (if provided) or calls <see cref="IPageModelBase{TActionResult}" /> to process result generically
        /// Do not process any further registered methods if any error or exception occured
        /// </summary>
        /// <param name="logger">Logger to be used in evaluation process</param>
        /// <param name="responseComposer">Helper class to create correct <see cref="IResponse{TResult}"/> objects</param>
        /// <param name="onSuccess">Optional success handler</param>
        /// <param name="onError">Optional error handler</param>
        /// <returns></returns>
        Task<IResponse<TResult>> Evaluate(
            IObjectLogger logger,
            IResponseComposer responseComposer,
            Func<TResult, IResponse<TResult>>? onSuccess = null,
            Func<IResponse<TResult>, IResponse<TResult>>? onError = null);

        /// <summary>
        /// Starts recursive evaluation of all chain of <see cref="Get{TToResult}"/> and <see cref="Process"/> methods registered before
        /// When result or error is calculated, calls <see cref="onSuccess"/> or <see cref="onError"/> parameters accordingly (if provided) or calls <see cref="IPageModelBase{TActionResult}" /> to process result generically
        /// Do not process any further registered methods if any error or exception occured
        /// </summary>
        /// <param name="logger">Logger to be used in evaluation process</param>
        /// <param name="responseComposer">Helper class to create correct <see cref="IResponse{TResult}"/> objects</param>
        /// <param name="onSuccess">Optional success handler</param>
        /// <param name="onError">Optional error handler</param>
        /// <returns></returns>
        Task Execute(
            IObjectLogger logger,
            IResponseComposer responseComposer,
            Action<TResult>? onSuccess = null,
            Action<IResponse<TResult>>? onError = null);
        
        /// <summary>
        /// Starts recursive evaluation of all chain of <see cref="Get{TToResult}"/> and <see cref="Process"/> methods registered before
        /// When result or error is calculated, calls <see cref="onSuccess"/> or <see cref="onError"/> parameters accordingly (if provided) or calls <see cref="IPageModelBase{TActionResult}" /> to process result generically
        /// Do not process any further registered methods if any error or exception occured
        /// </summary>
        /// <param name="page">Page or controller context of request</param>
        /// <param name="responseComposer">Helper class to create correct <see cref="IResponse{TResult}"/> objects</param>
        /// <param name="onSuccess">Optional success handler</param>
        /// <param name="onError">Optional error handler</param>
        /// <returns></returns>
        Task<TActionResult> Evaluate<TPage>(
            TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, Task<TActionResult>>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, Task<TActionResult>>? onError = null)
            where TPage : IPageModelBase<TActionResult>;
    }
}