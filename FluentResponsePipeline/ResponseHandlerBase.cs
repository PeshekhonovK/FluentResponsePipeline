using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal abstract class ResponseHandlerBase<TResult, TActionResult> : IEvaluator<TResult>
    {
        private bool IsLocked { get; set; } = false;

        private bool IsTransform { get; set; } = false;

        private bool IsTry { get; set; } = false;

        protected void VerifyAndLock()
        {
            if (this.IsLocked)
            {
                throw new InvalidOperationException($"This handler already has a child");
            }
            
            this.IsLocked = true;
        }
        
        protected internal virtual IResponse<TResponse> ProcessResponse<TResponse>(IObjectLogger logger,
            IResponse<TResponse> response)
        {
            Debug.Assert(logger != null);
            Debug.Assert(response != null);
                
            if (!(response is EmptyResponse<TResponse>))
            {
                HandleResponse(logger, response);
            }

            return response;
        }

        private static void HandleResponse(IObjectLogger logger, IResponse response)
        {
            if (!response.Succeeded)
            {
                logger.LogError(response);
            }
            else
            {
                logger.LogTrace(response);
            }
        }

        protected internal virtual async Task<TActionResult> ApplyToPage<TPage>(
            IResponse<TResult> response, 
            TPage page, 
            Func<TResult, TPage, Task<TActionResult>>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, Task<TActionResult>>? onError = null)
            where TPage : IPageModelBase<TActionResult>
        {
            if (response.Succeeded && onSuccess != null)
            {
                return await onSuccess(response.Payload, page);
            }

            if (!response.Succeeded && onError != null)
            {
                return await onError(response, page);
            }

            return page.Return(response);
        }
        
        protected static async Task<IResponse<TResult>> Try(IResponse<TResult> response, Func<TResult, Task<IResponse>> request, IResponseComposer responseComposer, IObjectLogger logger)
        {
            var result = await request(response.Payload);
            
            HandleResponse(logger, result);

            return result.Succeeded 
                ? response 
                : responseComposer.From<TResult>(result);
        }

        protected void SetTransform()
        {
            this.IsTransform = true;
        }

        protected void SetTry()
        {
            this.IsTry = true;
        }

        protected void VerifyNotTransform()
        {
            if (this.IsTransform)
            {
                throw new InvalidOperationException("This get already has a transform");
            }
        }

        protected void VerifyNotTry()
        {
            if (this.IsTry)
            {
                throw new InvalidOperationException("This get already has a try logic");
            }
        }

        public abstract Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer);

        public async Task<TActionResult> Evaluate<TPage>(
            TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
            where TPage : IPageModelBase<TActionResult>
        {
            var onSuccessAsync = onSuccess != null ? new Func<TResult, TPage, Task<TActionResult>>((r, p) => Task.FromResult(onSuccess(r, p))) : null;
            var onErrorAsync = onError != null ? new Func<IResponse<TResult>, TPage, Task<TActionResult>>((r, p) => Task.FromResult(onError(r, p))) : null;

            return await this.EvaluateAsync(page, responseComposer, onSuccessAsync, onErrorAsync);
        }

        public async Task<IResponse<TResult>> Evaluate(
            IObjectLogger logger,
            IResponseComposer responseComposer,
            Func<TResult, IResponse<TResult>>? onSuccess = null,
            Func<IResponse<TResult>, IResponse<TResult>>? onError = null)
        {
            var response = await this.GetResult(logger, responseComposer);

            Debug.Assert(response != null);
            
            if (response.Succeeded && onSuccess != null)
            {
                return onSuccess(response.Payload);
            }

            if (!response.Succeeded && onError != null)
            {
                return onError(response);
            }

            return response;
        }

        public async Task<IResponse> Execute(
            IObjectLogger logger,
            IResponseComposer responseComposer,
            Func<TResult, IResponse>? onSuccess = null,
            Func<IResponse<TResult>, IResponse>? onError = null)
        {
            var response = await this.GetResult(logger, responseComposer);

            Debug.Assert(response != null);
            
            if (response.Succeeded && onSuccess != null)
            {
                return onSuccess(response.Payload);
            }

            if (!response.Succeeded && onError != null)
            {
                return onError(response);
            }

            return response;
        }

        public async Task<TActionResult> EvaluateAsync<TPage>(TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, Task<TActionResult>>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, Task<TActionResult>>? onError = null) 
            where TPage : IPageModelBase<TActionResult>
        {
            Debug.Assert(page != null);
            
            var result = await this.GetResult(page.Logger, responseComposer);
            
            Debug.Assert(result != null);

            return await this.ApplyToPage(result, page, onSuccess, onError);
        }
    }
}