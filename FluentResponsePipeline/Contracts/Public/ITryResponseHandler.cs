namespace FluentResponsePipeline.Contracts.Public
{
    public interface ITryResponseHandler<out TResult, TActionResult> 
        : IBaseResponseHandler<TResult, TActionResult>
    {
    }
}