using System;
using System.Threading.Tasks;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IGetResponseHandler<out TResult, TActionResult> : IBaseResponseHandler<TResult, TActionResult>
    {
        ITryResponseHandler<TResult, TActionResult> Try(Func<TResult, Task<IResponse>> request);
    }
}