using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline.Contracts.Internal
{
    internal interface IEvaluator<TResult>
    {
        Task<IResponse<TResult>> GetResult(IObjectLogger logger);
    }
}