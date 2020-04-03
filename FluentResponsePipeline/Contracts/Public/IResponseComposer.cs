using System;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IResponseComposer
    {
        IResponse<TResult> From<TParentResult, TResult>(IResponse<TParentResult> parentResult);
        IResponse<TResult> Success<TResult>(TResult handler);
        IResponse<TResult> Error<TResult>(Exception exception);
    }
}