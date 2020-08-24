using System;
using System.Threading.Tasks;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IGetResponseHandler<TResult, TActionResult> : IBaseResponseHandler<TResult, TActionResult>
    {
    }
}