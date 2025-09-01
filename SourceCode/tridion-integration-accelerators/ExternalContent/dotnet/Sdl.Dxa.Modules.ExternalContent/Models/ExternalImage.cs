using System;
using System.Collections.Specialized;
using System.Globalization;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Modules.ExternalContent.Models
{
    public class ExternalImage : EclItem
    {
        static public bool UseTemplateFragment { get; private set; } = false;
        static public bool GenerateResponsiveImages { get; private set; } = false;

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="configuration"></param>
        public static void Configure(NameValueCollection configuration)
        {
            
            var useTemplateFragment = configuration["externalcontent-image-use-template-fragment"];
            if ( useTemplateFragment != null )
            {
                UseTemplateFragment = bool.Parse(useTemplateFragment);
            }

            // TODO: Call this useDxaAspectRatioCrop instead?
            var generateResponsiveImages = configuration["externalcontent-image-generate-responsive-images"];
            if ( generateResponsiveImages != null )
            {
                GenerateResponsiveImages = bool.Parse(generateResponsiveImages);
            }
        }
        
        /// <summary>
        /// To HTML
        /// </summary>
        /// <param name="widthFactor"></param>
        /// <param name="aspect"></param>
        /// <param name="cssClass"></param>
        /// <param name="containerSize"></param>
        /// <returns></returns>
        public override string ToHtml(string widthFactor, double aspect = 0, string cssClass = null, int containerSize = 0)
        {
            if (UseTemplateFragment && !string.IsNullOrEmpty(EclTemplateFragment))
            {
                return base.ToHtml(widthFactor, aspect, cssClass, containerSize);
            }
                     
            var imageUrl = Url;
            return ToHtml(imageUrl, widthFactor, aspect, cssClass, containerSize);
        }

        public static string ToHtml(string imageUrl, string widthFactor, double aspect = 0, string cssClass = null,
            int containerSize = 0)
        {
            if ( imageUrl.StartsWith("/") && GenerateResponsiveImages )
            {
                imageUrl = SiteConfiguration.MediaHelper.GetResponsiveImageUrl(imageUrl, aspect, widthFactor, containerSize);
            }
            
            // Right now this is connected to the DXA Whitelabel design. 
            // TODO: Decouple this!!
            //
            string dataAspect = (Math.Truncate(aspect * 100) / 100).ToString(CultureInfo.InvariantCulture);
            string widthAttr = string.IsNullOrEmpty(widthFactor) ? null : string.Format("width=\"{0}\"", widthFactor);
            string classAttr = string.IsNullOrEmpty(cssClass) ? null : string.Format("class=\"{0}\"", cssClass);
            return string.Format("<img src=\"{0}\" data-aspect=\"{1}\" {2}{3}/>",
                imageUrl, dataAspect, widthAttr, classAttr);   
        }
        
        

        /// <summary>
        /// Gets the default View.
        /// </summary>
        /// <param name="localization">The context Localization</param>
        /// <remarks>
        /// This makes it possible possible to render "embedded" Image Models using the Html.DxaEntity method.
        /// </remarks>
        public override MvcData GetDefaultView(Localization localization)
        {
            return new MvcData("ExternalContent:ExternalImage");
        }
    }
}