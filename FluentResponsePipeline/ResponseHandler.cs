using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentResponsePipeline.Contracts.Internal;
using FluentResponsePipeline.Contracts.Public;

namespace FluentResponsePipeline
{
    internal class ResponseHandler<TFrom, TRequestResult, TResult, TActionResult> : 
        ResponseHandlerBase<TResult, TActionResult>, 
        ITryResponseHandler<TResult, TActionResult>,
        IResponseHandler<TFrom, TRequestResult, TResult, TActionResult>,
        IResponseHandlerWithTransform<TFrom, TRequestResult, TResult, TActionResult>
    {
        private IEvaluator<TFrom> Parent { get; }

        private Func<TFrom, Task<IResponse<TRequestResult>>> Request { get; }
        
        private Func<IResponse<TFrom>, IResponse<TRequestResult>, IResponseComposer, IObjectLogger, Task<IResponse<TResult>>> TransformFunc { get; }

        public ResponseHandler(
            IEvaluator<TFrom> parent, 
            Func<TFrom, Task<IResponse<TRequestResult>>> request,
            Func<IResponse<TFrom>, IResponse<TRequestResult>, IResponseComposer, IObjectLogger, Task<IResponse<TResult>>> transform)
        {
            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.TransformFunc = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public IResponseHandler<TResult, TToResult, TToResult, TActionResult> Get<TToResult>(Func<TResult, Task<IResponse<TToResult>>> request)
        {
            Debug.Assert(request != null);
            
            this.VerifyAndLock();

            return new ResponseHandler<TResult, TToResult, TToResult, TActionResult>(this, request, (source, response, composer, logger) => Task.FromResult(response));
        }

        private IResponseHandlerWithTransform<TFrom, TRequestResult, TResult, TActionResult> AsTransform()
        {
            base.SetTransform();

            return this;
        }

        public ITryResponseHandler<TResult,TActionResult> AsTry()
        {
            base.SetTry();

            return this;
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

        public IResponseHandlerWithTransform<TFrom, TRequestResult, TTransformResult, TActionResult> Transform<TTransformResult>(Func<IResponse<TFrom>, IResponse<TRequestResult>, Task<IResponse<TTransformResult>>> transform)
        {
            Debug.Assert(transform != null);
            
            this.VerifyAndLock();
            this.VerifyNotTransform();
            this.VerifyNotTry();

            return new ResponseHandler<TFrom, TRequestResult, TTransformResult, TActionResult>(
                    this.Parent, 
                    this.Request,
                    (source, response, composer, logger) => this.Transform(transform, source, response, logger))
                .AsTransform();
        }
        private  async Task<IResponse<TTransformResult>> Transform<TTransformResult>(Func<IResponse<TFrom>, IResponse<TRequestResult>, Task<IResponse<TTransformResult>>> transform, IResponse<TFrom> source, IResponse<TRequestResult> response, IObjectLogger logger)
        {
            return this.ProcessResponse(logger, await transform(source, response));
        }

        public IResponseHandlerWithTransform<TFrom, TRequestResult, TTransformResult, TActionResult> ReplaceTransform<TTransformResult>(Func<IResponse<TFrom>, IResponse<TRequestResult>, Task<IResponse<TTransformResult>>> transform)
        {
            return this.Transform(transform);
        }

        public override async Task<IResponse<TResult>> GetResult(IObjectLogger logger, IResponseComposer responseComposer)
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

                var response = this.ProcessResponse(logger, await this.GetResponse(parentResult, logger, responseComposer));

                return await this.TransformFunc(parentResult, response, responseComposer, logger);
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
                    : throw new InvalidOperationException("Provider is null");
            }
            catch (Exception e)
            {
                logger.LogCritical(e);
                return responseComposer.Error<TRequestResult>(e);
            }
        }
    }
}