using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal class FirstResponseProvider<TRequestResult, TResult, TActionResult> 
        : ResponseHandlerBase<TResult, TActionResult>,
            IEvaluator<TResult>, 
            IFirstResponseHandler<TRequestResult, TResult, TActionResult>,
            IFirstResponseHandlerWithTransform<TRequestResult, TResult, TActionResult>
    {
        private Func<Task<IResponse<TRequestResult>>> Request { get; }
        
        private Func<IResponse<TRequestResult>, IResponse<TResult>> TransformFunc { get; }
            
        public FirstResponseProvider(Func<Task<IResponse<TRequestResult>>> request, Func<IResponse<TRequestResult>, IResponse<TResult>> transform)
        {
            Debug.Assert(request != null);
                
            this.Request = request;
            this.TransformFunc = transform;
        }
        
        public virtual async Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(logger != null);
            
            try
            {
                var requestResult = await this.GetResponse(logger, responseComposer);

                Debug.Assert(requestResult != null);
                
                return this.ProcessResponse(logger, this.TransformFunc(requestResult));
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TResult>(e);
            }
        }

        private async Task<IResponse<TRequestResult>> GetResponse(IObjectLogger logger, IResponseComposer responseComposer)
        {
            try
            {
                return await this.Request();
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TRequestResult>(e);
            }
        }

        public IResponseHandler<TResult, TToResult, TToResult, TActionResult> Get<TToResult>(Func<TResult, Task<IResponse<TToResult>>> request)
        {
            Debug.Assert(request != null);
            
            this.VerifyAndLock();
            
            return new ResponseHandler<TResult, TToResult, TToResult, TActionResult>(this, request, (source, response) => response);
        }

        public IFirstResponseHandlerWithTransform<TRequestResult, TTransformResult, TActionResult> Transform<TTransformResult>(Func<IResponse<TRequestResult>, IResponse<TTransformResult>> transform)
        {
            Debug.Assert(transform != null);
            
            this.VerifyAndLock();

            return new FirstResponseProvider<TRequestResult, TTransformResult, TActionResult>(this.Request, transform);
        }

        public IFirstResponseHandlerWithTransform<TRequestResult, TTransformResult, TActionResult> ReplaceTransform<TTransformResult>(Func<IResponse<TRequestResult>, IResponse<TTransformResult>> transform)
        {
            return this.Transform(transform);
        }

        public async Task<TActionResult> Evaluate<TPage>(
            TPage page, 
            IResponseComposer responseComposer, 
            Func<TResult, TPage, TActionResult>? onSuccess = null, 
            Func<IResponse<TResult>, TPage, TActionResult>? onError = null)
            where TPage : IPageModelBase<TActionResult>
        {
            var result = await this.GetResult(page.Logger, responseComposer);
                
            Debug.Assert(result != null);

            return this.ApplyToPage(result, page, onSuccess, onError);
        }
    }
}