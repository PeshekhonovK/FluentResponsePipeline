using System.Net;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IPageModelBase<out TActionResult>
    {
        IObjectLogger Logger { get; }

        TActionResult Return<TResult>(IResponse<TResult> response);
    }
}