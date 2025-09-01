using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Modules.ExternalContent.Models
{
    /// <summary>
    /// Playable media, i.e. videos, play lists etc
    /// </summary>
    public class PlayableMedia : EclItem
    {
        public string GetPlayerSetting(string name)
        {
            // First use routeValues and then fall back on video metadata
            //
            if (MvcData.RouteValues != null)
            {
                if (MvcData.RouteValues.ContainsKey(name))
                {
                    return MvcData.RouteValues[name];
                }
            }

            if (EclExternalMetadata.ContainsKey(name))
            {
                return (string) EclExternalMetadata[name];
            }
            
            // TODO: How can we get internal metadata here as well? Do we need to add this as a semantic property?
            
            return null;
        }
        
        
        /// <summary>
        /// Gets the default View.
        /// </summary>
        /// <param name="localization">The context Localization</param>
        /// <remarks>
        /// This makes it possible possible to render "embedded" video Models using the Html.DxaEntity method.
        /// </remarks>
        public override MvcData GetDefaultView(Localization localization)
        {
            return new MvcData("ExternalContent:ExternalVideo");
        }
    }
}