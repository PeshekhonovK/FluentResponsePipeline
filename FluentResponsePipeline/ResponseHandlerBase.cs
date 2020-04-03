using System;
using System.Diagnostics;
using System.Net;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal class ResponseHandlerBase<TResult, TActionResult>
    {
        protected static IResponse<TResult> ProcessResponse(IObjectLogger logger, IResponse<TResult> response)
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

        protected static TActionResult ApplyToPage<TPage>(
            IResponse<TResult> result, 
            TPage page, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
            where TPage : IPageModelBase<TActionResult>
        {
            if (result.StatusCode == HttpStatusCode.OK && onSuccess != null)
            {
                return onSuccess(result.Payload, page);
            }

            if (result.StatusCode != HttpStatusCode.OK && onError != null)
            {
                return onError(result, page);
            }

            return page.Return(result);
        }
    }
}