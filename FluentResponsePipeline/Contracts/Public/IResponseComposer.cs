using System;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IResponseComposer
    {
        IResponse From(IResponse parentResult);
        IResponse<TResult> From<TResult>(IResponse parentResult);
        IResponse<TResult> From<TParentResult, TResult>(IResponse<TParentResult> parentResult);
        IResponse<TResult> Success<TResult>(TResult result);
        IResponse<TResult> Error<TResult>(Exception exception);
    }
}