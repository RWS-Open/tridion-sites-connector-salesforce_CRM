using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Client.Models;
using Sdl.Dxa.Modules.ExternalContent.Models;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Models;
using Sdl.Web.Mvc.Configuration;
using Sdl.Web.Mvc.Controllers;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Modules.ExternalContent.Controllers
{
    /// <summary>
    /// External Content Controller.
    /// </summary>
    public class ExternalContentController : EntityController
    {
        private static readonly Regex BINARY_REFERENCE_REGEX = new Regex("(" + BinaryReferenceUrl.URL_PROTOCOL +  "[^\"]+)", RegexOptions.Compiled);
        
        protected override ViewModel EnrichModel(ViewModel model)
        {
            // TODO: Fix localization when the Xillio connector supports it
            
            var externalContent = base.EnrichModel(model) as Models.ExternalContent;

            if (externalContent.IsImage)
            {
                return externalContent;
            }
            
            if (WebRequestContext.Localization.IsStaticContentUrl(externalContent.Url)) // Content is published as value 
            {
                var multipartContentItem =
                    SiteConfiguration.ContentProvider.GetStaticContentItem(externalContent.Url,
                        WebRequestContext.Localization);
                var serializedContent = new SerializedExternalContent(multipartContentItem.GetContentStream());

                // Deserialize content fields
                //
                externalContent.ContentFields = serializedContent.GetContentFields();

                // Deserialize HTML fragment
                //
                // TODO: Rich text handling??
                var htmlFragment = serializedContent.GetHtmlFragment();

                // Process embedded media
                //
                var encodedContentId = Convert.ToBase64String(Encoding.UTF8.GetBytes(externalContent.Url));
                ProcessEmbeddedMultimedia(externalContent.ContentFields, encodedContentId);
                externalContent.HtmlFragment = ProcessEmbeddedMultimediaInRichText(htmlFragment, encodedContentId);
            }
            else
            {
                // Resolve content run-times through the content service
                //
                var integrationClient = IntegrationApiClientProvider.Instance.Client;
                var contentEntity = integrationClient.GetEntity(externalContent.EclUri);

                // TODO: Read HTML fragment as well
                
                var contentFields = contentEntity.GetField("contentFields") as IList<NestedObject>;
                if (contentFields != null)
                {
                    externalContent.ContentFields = GetContentFields(contentFields);
                }

                externalContent.HtmlFragment = contentEntity.GetField("htmlFragment") as string;
            }

            return externalContent;
        }

        private IList<ContentField> GetContentFields(IList<NestedObject> contentNestedObjects)
        {
            var contentFields = new List<ContentField>();
            foreach (var contentNestedObject in contentNestedObjects)
            {
                var name = (string) contentNestedObject.GetField("name");
                string value;
                var binaryReference = (string) contentNestedObject.GetField("multimediaReference");
                if (binaryReference != null)
                {
                    value = binaryReference;
                }
                else
                {
                    value = (string) contentNestedObject.GetField("value");
                }

                ContentFieldType type;
                if (!ContentFieldType.TryParse((string) contentNestedObject.GetField("type"), true, out type))
                {
                    type = ContentFieldType.Unknown;
                }
                IList<ContentField> nestedFields = null;
                var nestedFieldList =  contentNestedObject.GetField("contentFields") as IList<NestedObject>;
                if (nestedFieldList != null)
                {
                    nestedFields = GetContentFields(nestedFieldList);
                }
                contentFields.Add(new ContentField
                {
                    Name = name,
                    Value = value,
                    Type = type,
                    ContentFields = nestedFields
                });
            }
            return contentFields;
        }

        private void ProcessEmbeddedMultimedia(IList<ContentField> contentFields, string encodedContentId)
        {
            foreach (var contentField in contentFields)
            {
                if (contentField.MultimediaReference != null)
                {
                    contentField.Value = ToImageUrl(contentField.MultimediaReference, encodedContentId);
                }

                if (contentField.Type == ContentFieldType.RichText)
                {
                    contentField.Value = ProcessEmbeddedMultimediaInRichText(contentField.Value, encodedContentId);
                }

                if (contentField.ContentFields != null)
                {
                    ProcessEmbeddedMultimedia(contentField.ContentFields, encodedContentId);
                }
            }
        }
        
        // TODO: Move this into the BinaryReferenceUrl class?
        private string ProcessEmbeddedMultimediaInRichText(string richText, string encodedContentId)
        {
            return BINARY_REFERENCE_REGEX.Replace(richText, 
                m => ToImageUrl(new BinaryReferenceUrl(m.Value).BinaryReference, encodedContentId));
        }

        private string ToImageUrl(IBinaryReference binaryReference, string encodedContentId)
        {
            return WebRequestContext.Localization.GetBaseUrl() + "/externalcontent/embedded/image/" +
                encodedContentId + "/" +binaryReference.Id.Replace("/", "_");
        }
    }
}