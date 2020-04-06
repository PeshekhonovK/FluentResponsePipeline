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
    }
}