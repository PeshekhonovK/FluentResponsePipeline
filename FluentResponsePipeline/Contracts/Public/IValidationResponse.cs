using System.Collections.Generic;

namespace FluentResponsePipeline.Contracts.Public
{
    public interface IValidationResponse : IResponse
    {
        IEnumerable<string> Errors { get; }
    }
}