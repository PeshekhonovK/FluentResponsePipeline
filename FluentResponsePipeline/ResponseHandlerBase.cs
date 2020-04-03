using System;
using System.Diagnostics;
using System.Net;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal abstract class ResponseHandlerBase<TResult, TActionResult>
    {
        protected internal static IResponse<TResult> ProcessResponse(IObjectLogger logger, IResponse<TResult> response)
        {
            Debug.Assert(logger != null);
            Debug.Assert(response != null);
                
            if (!response.Succeeded)
            {
                logger.LogError(response);
            }
            else
            {
                logger.LogTrace(response);
            }

            return response;
        }

        protected internal static TActionResult ApplyToPage<TPage>(
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
    }
}