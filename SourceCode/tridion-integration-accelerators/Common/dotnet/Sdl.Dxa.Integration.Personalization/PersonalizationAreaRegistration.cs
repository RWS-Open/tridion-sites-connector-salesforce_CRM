using Sdl.Dxa.Integration.Client.Processor;
using Sdl.Dxa.Integration.Personalization.Processor;
using Sdl.Dxa.Modules.Crm;
using Sdl.Web.Mvc.Configuration;

namespace Sdl.Dxa.Integration.Personalization
{
    /// <summary>
    /// Area registration for the Personalization module
    /// </summary>
    public class PersonalizationAreaRegistration : BaseAreaRegistration
    {
        const string CONTROLLER_NAMESPACE = "Sdl.Dxa.Integration.Personalization.Controller";

        public override string AreaName => "Personalization";

        protected override void RegisterAllViewModels()
        {
            //RegisterViewModel("StandaloneRegion", typeof(PageModel), "StandaloneRegion");
            
            DIRegisty.Register<IRequestProcessor>(new MarketoLeadProcessor());
            DIRegisty.Register<IRequestProcessor>(new SFMCContactProcessor());
        }
    }
}
