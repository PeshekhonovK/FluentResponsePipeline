using System;
using System.Net;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline.Contracts.Internal
{
    internal class EmptyResponse<TResult> : IResponse<TResult>
    {
        private const string Error = "This response is just a stub, you should not ever use it";
        
        public bool Succeeded => throw new InvalidOperationException(Error); 
        public HttpStatusCode StatusCode => throw new InvalidOperationException(Error); 
        public string Message => throw new InvalidOperationException(Error); 
        public TResult Payload => throw new InvalidOperationException(Error);

        public static Task<IResponse<TResult>> AsTask()
        {
            return Task.FromResult((IResponse<TResult>) new EmptyResponse<TResult>());
        }
    }
}