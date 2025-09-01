using System.Web.Mvc;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Client.Processor;
using Sdl.Dxa.Integration.Form.Models;
using Sdl.Dxa.Modules.Crm;
using Sdl.Web.Mvc.Configuration;

namespace Sdl.Dxa.Integration.Form
{
    public class IntegrationFormAreaRegistration : BaseAreaRegistration
    {
        const string CONTROLLER_NAMESPACE = "Sdl.Dxa.Integration.Form.Controller";
        
        public override string AreaName => "IntegrationForm";

        protected override void RegisterAllViewModels()
        {
            TypeRegistrationHelper.BuildAndRegisterSubTypes("form-field-namespaces", typeof(FormFieldEclItem));
            TypeRegistrationHelper.BuildAndRegisterSubTypes("eclform-namespaces", typeof(EclForm));
            
            // Entity Views
            //
            RegisterViewModel("Form", typeof(IntegrationForm), "FormRender");
            RegisterViewModel("RequestInfo", typeof(IntegrationForm), "FormRender");
            RegisterViewModel("InvisibleFormWidget", typeof(InvisibleFormWidget), "InvisibleWidget");
            RegisterViewModel("EclForm", typeof(EclForm));
        }
    }
}