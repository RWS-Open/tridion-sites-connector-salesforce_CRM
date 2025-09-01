using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Client.Processor;
using Sdl.Dxa.Modules.Crm.Models;
using Sdl.Dxa.Modules.Crm.Tracking;
using Sdl.Web.Mvc.Configuration;

namespace Sdl.Dxa.Modules.Crm
{
    /// <summary>
    /// Area registration for the CRM module
    /// </summary>
    public class CrmAreaRegistration : BaseAreaRegistration
    {
        const string CONTROLLER_NAMESPACE = "Sdl.Dxa.Modules.Crm.Controller";
        
        public override string AreaName => "CRM";

        protected override void RegisterAllViewModels()
        {
            
            // Register form handlers
            //

            // Integration interceptors
            //
            // TODO: Have CRM entity types configurable!!!!
            DIRegisty.Register<IIntegrationInterceptor>(new TrackingInterceptor());
            DIRegisty.Register<IIntegrationInterceptor>(new EntitySessionInterceptor(new [] {"Contact"}));
            
            // Tracking Handlers
            //
            DIRegisty.Register<ITrackingHandler>(new ADFTrackingHandler());
            DIRegisty.Register<ITrackingHandler>(new CRMVisitorTrackingHandler());
            
            // Entity Views
            //
            RegisterViewModel("TrackingWidget", typeof(TrackingWidget));
        }
    }
}