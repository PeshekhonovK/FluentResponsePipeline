using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal class ResponseHandler<TFrom, TResult, TActionResult> : 
        ResponseHandlerBase<TResult, TActionResult>, 
        IEvaluator<TResult>,
        IResponseHandler<TResult, TActionResult>
    {
        private IEvaluator<TFrom> Parent { get; }

        private Func<TFrom, Task<IResponse<TResult>>>? Request { get; }

        private Func<TFrom, TResult>? Handler { get; }

        public ResponseHandler(IEvaluator<TFrom> parent, Func<TFrom, Task<IResponse<TResult>>> request)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public ResponseHandler(IEvaluator<TFrom> parent, Func<TFrom, TResult> handler)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public IResponseHandler<TToResult, TActionResult> With<TToResult>(Func<TResult, Task<IResponse<TToResult>>> request)
        {
            Debug.Assert(request != null);

            return new ResponseHandler<TResult, TToResult, TActionResult>(this, request);
        }

        public IResponseHandler<TToResult, TActionResult> Process<TToResult>(Func<TResult, TToResult> handler)
        {
            Debug.Assert(handler != null);

            return new ResponseHandler<TResult, TToResult, TActionResult>(this, handler);
        }

        public async Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(this.Parent != null);
            Debug.Assert(this.Request != null || this.Handler != null);

            var parentResult = await this.Parent.GetResult(logger, responseComposer);

            if (!parentResult.Succeeded)
            {
                return responseComposer.From<TFrom, TResult>(parentResult);
            }

            var response = await this.GetResponse(parentResult, logger, responseComposer);

            return this.ProcessResponse(logger, response);
        }

        private async Task<IResponse<TResult>> GetResponse(IResponse<TFrom> parentResult, IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(parentResult != null);

            try
            {
                return this.Request != null
                    ? await this.Request(parentResult.Payload)
                    : this.Handler != null
                        ? responseComposer.Success<TResult>(this.Handler(parentResult.Payload))
                        : throw new InvalidOperationException($"Both Provider and Handler are null");
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TResult>(e);
            }
        }

        public async Task<TActionResult> Evaluate<TPage>(
            TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
            where TPage : IPageModelBase<TActionResult>
        {
            Debug.Assert(page != null);

            var result = await this.GetResult(page.Logger, responseComposer);

            Debug.Assert(result != null);

            return this.ApplyToPage(result, page, onSuccess, onError);
        }
    }
}