using Sdl.Dxa.Modules.Crm;
using Sdl.Dxa.Modules.ECommerce.Models;
using Sdl.Web.Mvc.Configuration;

namespace Sdl.Dxa.Modules.ECommerce
{
    public class ECommerceAreaRegistration : BaseAreaRegistration
    {
        public override string AreaName => "ECommerce";

        protected override void RegisterAllViewModels()
        {
            TypeRegistrationHelper.BuildAndRegisterSubTypes("ecommerce-namespaces", typeof(Product));
            
            // Entity Views
            //
            RegisterViewModel("Product", typeof(Product), "Product");
            
        }
    }
}