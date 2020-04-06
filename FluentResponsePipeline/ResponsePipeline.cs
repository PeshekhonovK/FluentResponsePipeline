using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    public static class ResponsePipeline<TActionResult>
    {
        /// <summary>
        /// Returns response provider that can be chained to get more data using this response, somehow process this response or evaluate all the method chain
        /// </summary>
        /// <param name="request">Wrapper with a response source call</param>
        /// <typeparam name="TResult">Type of response payload, no restrictions</typeparam>
        public static IFirstResponseHandler<TResult, TResult, TActionResult> With<TResult>(Func<Task<IResponse<TResult>>> request)
        {
            Debug.Assert(request != null);

            return new FirstResponseProvider<TResult, TResult, TActionResult>(request, response => response);
        }
    }
}