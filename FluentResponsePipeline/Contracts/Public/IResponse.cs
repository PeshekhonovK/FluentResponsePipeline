using System.Net;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IResponse
    {
        public bool Succeeded { get; }
        
        public HttpStatusCode StatusCode { get; }
        
        public string Message { get; }
    }
    
    public interface IResponse<out TResult> 
        : IResponse
    {
        public TResult Payload { get; }
    }
}