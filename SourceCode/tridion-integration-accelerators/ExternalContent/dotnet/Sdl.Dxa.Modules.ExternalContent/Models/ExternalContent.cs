using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sdl.Dxa.Integration.Client.Models;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Contracts;
using MvcData = Sdl.Web.Common.Models.MvcData;

namespace Sdl.Dxa.Modules.ExternalContent.Models
{
    public class ExternalContent : EclItem
    {
        private static readonly Regex VariableRegex = new Regex("%([^%]+)%", RegexOptions.Compiled);
        
        // TODO: Part of standard base class instead??
        
        private IEntityIdentity _identity;

        public IEntityIdentity Identity
        {
            get
            {
                if (_identity == null)
                {
                    _identity = new EclUri(EclUri);
                }

                return _identity;
            }
        }

        public string Title => GetEclExternalMetadataValue("Title") as string;

        // TODO: Genenalize so it applies for more than Xillio
        public bool IsImage => Identity.Type.Equals("XillioMedia", StringComparison.OrdinalIgnoreCase);
        
        public string HtmlFragment { get; set; } // TODO: Should this be RichText??
        
        public IList<ContentField> ContentFields { get; set; } = new List<ContentField>();

        public string GetNormalizedContent(string name)
        {
            return ContentFields.FirstOrDefault(c => c.Name.Equals(name))?.Value;
        }

        public override string ToHtml(string widthFactor, double aspect = 0, string cssClass = null, int containerSize = 0)
        {
            if (IsImage)
            {
                return ExternalImage.ToHtml(Url, widthFactor, aspect, cssClass, containerSize);
            }

            return HtmlFragment;
            /*
            // Default behaviour when there is no specific template consuming the normalized fields (or just use the HTML fragment??
            //
            var html = new StringBuilder();
            foreach (var content in ContentFields)
            {
                if (content.Type == ContentFieldType.RichText)
                {
                    var richText = ProcessRichText(content.Value);
                    html.Append("<div>" + richText + "</div>");
                }
                else if (content.Type == ContentFieldType.Multimedia)
                {
                    // For now assume it's always an image
                    //
                    html.Append(ExternalImage.ToHtml(content.Value, widthFactor, aspect, cssClass, containerSize));
                }
            }

            return html.ToString();
            */
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

        private string ProcessRichText(string richText)
        {
            // TODO: This should be updated to parse binref URLs instead!!! Should this be made by the client instead????
            return VariableRegex.Replace(richText, m => Substitute(m.Value.Replace("%", "")));
        }
        
        private string Substitute(string variable)
        {
            var content = ContentFields.FirstOrDefault(c => c.Name.Equals(variable));
            return content?.Value;
        }
    }
}