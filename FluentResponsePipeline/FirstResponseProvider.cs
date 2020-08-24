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
        
        private Func<IResponse<TRequestResult>, IResponseComposer, IObjectLogger, Task<IResponse<TResult>>> TransformFunc { get; }
            
        public FirstResponseProvider(Func<Task<IResponse<TRequestResult>>> request, Func<IResponse<TRequestResult>, IResponseComposer, IObjectLogger, Task<IResponse<TResult>>> transform)
        {
            Debug.Assert(request != null);
                
            this.Request = request;
            this.TransformFunc = transform;
        }

        private FirstResponseProvider<TRequestResult, TResult, TActionResult> AsTransform()
        {
            base.SetTransform();

            return this;
        }
        
        public override async Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer)
        {
            Debug.Assert(logger != null);
            
            try
            {
                var requestResult = this.ProcessResponse(logger,await this.GetResponse(logger, responseComposer));

                Debug.Assert(requestResult != null);
                
                return await this.TransformFunc(requestResult, responseComposer, logger);
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

            return new ResponseHandler<TResult, TToResult, TToResult, TActionResult>(this, request, (source, response, composer, logger) => Task.FromResult(response));
        }

        public ITryResponseHandler<TResult, TActionResult> Try(Func<TResult, Task<IResponse>> request)
        {
            Debug.Assert(request != null);
            
            this.VerifyAndLock();
            
            return new ResponseHandler<TResult, TResult, TResult, TActionResult>(
                this, 
                result => EmptyResponse<TResult>.AsTask(), 
                (source, response, composer, logger) => Try(source, request, composer, logger))
                .AsTry();
        }

        public IFirstResponseHandlerWithTransform<TRequestResult, TTransformResult, TActionResult> Transform<TTransformResult>(Func<IResponse<TRequestResult>, Task<IResponse<TTransformResult>>> transform)
        {
            Debug.Assert(transform != null);
            
            this.VerifyAndLock();
            this.VerifyNotTry();
            this.VerifyNotTransform();

            return new FirstResponseProvider<TRequestResult, TTransformResult, TActionResult>(
                    this.Request, 
                    (response, composer, logger) => this.Transform(transform, response, logger))
                .AsTransform();
        }

        private async Task<IResponse<TTransformResult>> Transform<TTransformResult>(Func<IResponse<TRequestResult>, Task<IResponse<TTransformResult>>> transform, IResponse<TRequestResult> response, IObjectLogger logger)
        {
            return this.ProcessResponse(logger, await transform(response));
        }

        public IFirstResponseHandlerWithTransform<TRequestResult, TTransformResult, TActionResult> ReplaceTransform<TTransformResult>(Func<IResponse<TRequestResult>, Task<IResponse<TTransformResult>>> transform)
        {
            return this.Transform(transform);
        }
    }
}