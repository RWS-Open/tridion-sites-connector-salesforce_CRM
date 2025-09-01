using System;
using System.Text;
using System.Web.Mvc;
using Sdl.Dxa.Modules.ExternalContent.Models;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Logging;
using Sdl.Web.Mvc.Configuration;
using Sdl.Web.Mvc.Formats;

namespace Sdl.Dxa.Modules.ExternalContent.Controllers
{
    public class EmbeddedImageController : Controller
    {
        // TODO: Is /externalcontent/ a good controller path???
        [Route("externalcontent/embedded/image/{contentId}/{mediaId}")]
        [Route("{localization}/externalcontent/embedded/image/{contentId}/{mediaId}")]
        [FormatData]
        public virtual ActionResult GetEmbeddedImage(string contentId, string mediaId)
        {
            Log.Info($"Getting embedded image with content ID: '{contentId}' and media ID: '{mediaId}'");
            var decodedContentId = Encoding.UTF8.GetString(Convert.FromBase64String(contentId));
            Log.Info("Decoded content ID: " + decodedContentId);
            var contentItem =
                SiteConfiguration.ContentProvider.GetStaticContentItem(decodedContentId,
                    WebRequestContext.Localization);
            var serializedContent  = new SerializedExternalContent(contentItem.GetContentStream());
            Log.Info("Serialized Content created...");
            var media = serializedContent.GetMultimedia(mediaId);
            if (media == null)
            {
                return HttpNotFound($"No embedded image with ID '{mediaId}' was not found!");
            }
            Log.Info("Found multimedia with content type: " + media.ContentType);
            
            // TODO: Add cache headers etc
            // Reuse mechanism from Instant Campaign

            return File(media.Data, media.ContentType);
        }
    }
}