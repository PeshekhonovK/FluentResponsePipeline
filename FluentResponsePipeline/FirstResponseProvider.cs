using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal class FirstResponseProvider<TResult, TActionResult> 
        : ResponseHandlerBase<TResult, TActionResult>,
            IEvaluator<TResult>, 
            IResponseHandler<TResult, TActionResult>
    {
        private Func<Task<IResponse<TResult>>> Request { get; }
            
        public FirstResponseProvider(Func<Task<IResponse<TResult>>> request)
        {
            Debug.Assert(request != null);
                
            this.Request = request;
        }
            
        public virtual async Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(logger != null);
                
            try
            {
                return this.ProcessResponse(logger, await this.Request());
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TResult>(e);
            }
        }
        
        public IResponseHandler<TChildTo, TActionResult> Process<TChildTo>(Func<TResult, TChildTo> handler)
        {
            Debug.Assert(handler != null);

            return new ResponseHandler<TResult, TChildTo, TActionResult>(this, handler);
        }

        public IResponseHandler<TResultTo, TActionResult> With<TResultTo>(Func<TResult, Task<IResponse<TResultTo>>> request)
        {
            Debug.Assert(request != null);

            return new ResponseHandler<TResult, TResultTo, TActionResult>(this, request);
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