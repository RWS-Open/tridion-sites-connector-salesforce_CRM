using System;
using System.Web;
using Sdl.Dxa.Modules.Crm;

namespace Sdl.Dxa.Integration.Client.Processor
{
    /// <summary>
    /// Request Processor Module
    /// </summary>
    public class RequestProcessorModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += BeginRequest;
            context.EndRequest += EndRequest;
        }

        public void Dispose()
        {
        }

        private void BeginRequest(Object source, EventArgs e)
        {
            var application = (HttpApplication) source;
            var requestProcessors = DIRegisty.GetList<IRequestProcessor>();
            foreach (var requestProcessor in requestProcessors)
            {
                requestProcessor.PreProcess(application.Request);
            }
        }

        private void EndRequest(Object source, EventArgs e)
        {
            var application = (HttpApplication) source;
            var requestProcessors = DIRegisty.GetList<IRequestProcessor>();
            foreach (var requestProcessor in requestProcessors)
            {
                requestProcessor.PostProcess(application.Request, application.Response);
            }
        }
    }
}