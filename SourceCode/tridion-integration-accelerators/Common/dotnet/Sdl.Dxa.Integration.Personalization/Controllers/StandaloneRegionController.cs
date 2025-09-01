using System;
using System.Web.Mvc;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Mvc.Configuration;
using Sdl.Web.Mvc.Controllers;
using Sdl.Web.Mvc.Formats;

namespace Sdl.Dxa.Integration.Personalization.Controllers
{
    /// <summary>
    /// Standalone Region Controller. Is used to provide a region standalone from the page. Is primarily used for personalization scenarios
    /// when CDN is used to cache pages.
    /// </summary>
    public class StandaloneRegionController : PageController
    {
        [Route("xo-region/{pageId}/{region}")]
        [Route("{localization}/xo-region/{pageId}/{region}")]
        [FormatData]
        public virtual ActionResult Region(string pageId, string region)
        {
            var pageUrl = SiteConfiguration.LinkResolver.ResolveLink(string.Format("tcm:{0}-{1}-64", WebRequestContext.Localization.Id, pageId));
            var pageModel = ContentProvider.GetPageModel(pageUrl, WebRequestContext.Localization, false);

            pageModel.Regions.RemoveWhere(r => !r.Name.Equals(region, StringComparison.OrdinalIgnoreCase));
            ViewData["RegionName"] = region;
            
            HttpContext.Items["StandaloneRegion"] = true;

            // TODO: Improve the view routing to avoid absolute paths to it. 
            return View("~/Areas/Personalization/Views/StandaloneRegion/StandaloneRegion.cshtml", pageModel);
            
        }
        
    }
}