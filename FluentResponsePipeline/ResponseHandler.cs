using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal class ResponseHandler<TFrom, TRequestResult, TResult, TActionResult> : 
        ResponseHandlerBase<TResult, TActionResult>, 
        IEvaluator<TResult>,
        IResponseHandler<TFrom, TRequestResult, TResult, TActionResult>,
        IResponseHandlerWithTransform<TFrom, TRequestResult, TResult, TActionResult>
    {
        private IEvaluator<TFrom> Parent { get; }

        private Func<TFrom, Task<IResponse<TRequestResult>>> Request { get; }
        
        private Func<IResponse<TFrom>, IResponse<TRequestResult>, IResponse<TResult>> Transform { get; }

        public ResponseHandler(IEvaluator<TFrom> parent, Func<TFrom, Task<IResponse<TRequestResult>>> request, Func<IResponse<TFrom>, IResponse<TRequestResult>, IResponse<TResult>> transform)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public IResponseHandler<TResult, TToResult, TToResult, TActionResult> With<TToResult>(Func<TResult, Task<IResponse<TToResult>>> request)
        {
            Debug.Assert(request != null);
            
            this.VerifyAndLock();

            return new ResponseHandler<TResult, TToResult, TToResult, TActionResult>(this, request, (source, response) => response);
        }

        public IResponseHandlerWithTransform<TFrom, TRequestResult, TTransformResult, TActionResult> AddTransform<TTransformResult>(Func<IResponse<TFrom>, IResponse<TRequestResult>, IResponse<TTransformResult>> transform)
        {
            Debug.Assert(transform != null);
            
            this.VerifyAndLock();

            return new ResponseHandler<TFrom, TRequestResult, TTransformResult, TActionResult>(this.Parent, this.Request, transform);
        }

        public IResponseHandlerWithTransform<TFrom, TRequestResult, TTransformResult, TActionResult> ReplaceTransform<TTransformResult>(Func<IResponse<TFrom>, IResponse<TRequestResult>, IResponse<TTransformResult>> transform)
        {
            return this.AddTransform(transform);
        }

        public async Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(this.Parent != null);
            Debug.Assert(this.Request != null);

            try
            {
                var parentResult = await this.Parent.GetResult(logger, responseComposer);

                if (!parentResult.Succeeded)
                {
                    return responseComposer.From<TFrom, TResult>(parentResult);
                }

                var response = await this.GetResponse(parentResult, logger, responseComposer);

                var transformedResult = this.Transform(parentResult, response);

                return this.ProcessResponse(logger, transformedResult);
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TResult>(e);
            }
        }

        private async Task<IResponse<TRequestResult>> GetResponse(IResponse<TFrom> parentResult, IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(parentResult != null);

            try
            {
                return this.Request != null
                    ? await this.Request(parentResult.Payload)
                    : throw new InvalidOperationException($"Both Provider is null");
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TRequestResult>(e);
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