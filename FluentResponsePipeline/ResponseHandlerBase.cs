using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal abstract class ResponseHandlerBase<TResult, TActionResult>
    {
        private bool IsLocked { get; set; } = false;

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

        protected internal virtual TActionResult ApplyToPage<TPage>(
            IResponse<TResult> response, 
            TPage page, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
            where TPage : IPageModelBase<TActionResult>
        {
            if (response.Succeeded && onSuccess != null)
            {
                return onSuccess(response.Payload, page);
            }

            if (!response.Succeeded && onError != null)
            {
                return onError(response, page);
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
    }
}