using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sdl.Dxa.Modules.Crm
{
    // TODO: Move this to Accelerator Common
    // TODO: Is this needed really??
    
    /// <summary>
    /// Route Helper
    /// </summary>
    public class RouteHelper
    {
        /// <summary>
        /// Map route for a page controller. 
        /// As this is called after the global DXA initialization we have to shuffle around the route definition so it comes before the DXA page controller.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="defaults"></param>
        public static void MapRoute(string routeNamespace, RouteCollection routes, string name, string url, object defaults)
        {
            Route route = new Route(url, new MvcRouteHandler())
            {
                Defaults = CreateRouteValueDictionary(defaults),
                DataTokens = new RouteValueDictionary()
                {
                    { "Namespaces", routeNamespace}
                }
            };
            routes.Insert(0, route);
        }

        private static RouteValueDictionary CreateRouteValueDictionary(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            if (dictionary != null)
            {
                return new RouteValueDictionary(dictionary);
            }
            return new RouteValueDictionary(values);
        }
    }
}