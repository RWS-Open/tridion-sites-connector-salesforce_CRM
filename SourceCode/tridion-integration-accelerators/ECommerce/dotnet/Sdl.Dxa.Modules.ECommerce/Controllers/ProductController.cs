using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Modules.ECommerce.Models;
using Sdl.Web.Common.Models;
using Sdl.Web.Mvc.Controllers;

namespace Sdl.Dxa.Modules.ECommerce.Controllers
{
    public class ProductController : EntityController
    {
        protected override ViewModel EnrichModel(ViewModel model)
        {
            var product = base.EnrichModel(model) as Product;

            if (product != null)
            {
                var integrationClient = IntegrationApiClientProvider.Instance.Client;
                var productEntity = integrationClient.GetEntity(product.EclUri);
                product.enrichWithExternalData(productEntity);
            }

            return product;
        }
    }
}