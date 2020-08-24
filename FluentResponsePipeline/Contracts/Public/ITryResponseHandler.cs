namespace FluentResponsePipeline.Contracts.Public
{
    public interface ITryResponseHandler<TResult, TActionResult> 
        : IBaseResponseHandler<TResult, TActionResult>
    {
    }
}