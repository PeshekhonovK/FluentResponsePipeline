using System.Net;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline.Tests.Unit
{
    public class ResponseStub<TObject> : IResponse<TObject>
    {
        public bool Succeeded { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public TObject Payload { get; set; }

        public static IResponse<TObject> Success(TObject payload)
        {
            return new ResponseStub<TObject>
            {
                Payload = payload,
                Succeeded = true
            };
        }

        public static IResponse<TObject> Error(string message)
        {
            return new ResponseStub<TObject>
            {
                Message = message,
                Succeeded = false
            };
        }
    }
}