using System.Net;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline.Tests.Unit
{
    public class ResponseStub : IResponse
    {
        public bool Succeeded { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }

        public static IResponse Success()
        {
            return new ResponseStub
            {
                Succeeded = true
            };
        }

        public static IResponse Error(string message)
        {
            return new ResponseStub
            {
                Message = message,
                Succeeded = false
            };
        }
        
    }
    
    public class ResponseStub<TObject> : ResponseStub, IResponse<TObject>
    {
        public TObject Payload { get; set; }

        public static IResponse<TObject> Success(TObject payload)
        {
            return new ResponseStub<TObject>
            {
                Payload = payload,
                Succeeded = true
            };
        }

        public new static IResponse<TObject> Error(string message)
        {
            return new ResponseStub<TObject>
            {
                Message = message,
                Succeeded = false
            };
        }
    }
}