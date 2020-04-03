using System;
using System.Diagnostics;
using System.Net;

namespace FluentResponsePipeline.Contracts.Public
{
    public class Response<TResult> : IResponse<TResult>
    {
        public bool Succeeded { get; }
        public HttpStatusCode StatusCode { get; }
        public string Message { get; }
        public TResult Payload { get; }

        public static IResponse<TResult> Error(HttpStatusCode internalServerError, string eMessage)
        {
            throw new System.NotImplementedException();
        }

        public static IResponse<TResult> Success(TResult handler)
        {
            throw new System.NotImplementedException();
        }
        
        public Response(IResponse parentResult)
        {
            Debug.Assert(!parentResult.Succeeded);
            throw new System.NotImplementedException();
        }

        public static IResponse<TResult> Error(Exception internalServerError)
        {
            throw new NotImplementedException();
        }
    }
}