using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Newtonsoft.Json;
using Sdl.Web.DataModel;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.CommunicationManagement.Regions;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ExternalContentLibrary.V2;
using ComponentPresentation = Tridion.ContentManager.CommunicationManagement.ComponentPresentation;

namespace Sdl.Dxa.Integration.Tbb
{
    /// <summary>
    /// Extract ECL Metadata from component templates
    /// </summary>
    [TcmTemplateTitle("Extract Template ECL Metadata")]
    public class ExtractTemplateECLMetadata : ITemplate
    {
        private TemplatingLogger log =
            TemplatingLogger.GetLogger(typeof(ExtractTemplateECLMetadata));

        private const string EclMimeType = "application/externalcontentlibrary";

        /// <summary>
        /// Transform.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="package"></param>
        public void Transform(Engine engine, Package package)
        {
            log.Debug("Extracting ECL Metadata from link field");
            
            if (engine.PublishingContext.ResolvedItem.Item is Page)
            {
                log.Debug("Extracting from page...");
                var page = GetPage(engine, package);
                var eclMetadataFields = new List<EclMetadataField>();
                
                foreach (var cp in page.ComponentPresentations)
                {
                    ExtractMetadataFieldsFromComponentPresentation(cp, eclMetadataFields, engine);
                }
                ExtractMetadataFieldsFromRegions(page.Regions, eclMetadataFields, engine);

                if (eclMetadataFields.Count > 0)
                {
                    log.Debug("Processing the DXA page model");
                    var outputStr = package.GetByName(Package.OutputName).GetAsString();
                    var pageModel = JsonConvert.DeserializeObject<PageModelData>(outputStr, DataModelBinder.SerializerSettings);
                    foreach (var regionModel in pageModel.Regions)
                    {
                        ProcessRegionModel(regionModel, eclMetadataFields);
                    }
                    // Re-serialize the DXA JSON payload
                    //
                    log.Debug("Re-serializing the page model...");
                    package.PushItem(Package.OutputName,
                        package.CreateStringItem(ContentType.Text,
                            JsonSerialize(pageModel, IsPreview(engine), DataModelBinder.SerializerSettings)));
                }
                
            }
            else // if Component
            {
                var ct = GetComponentTemplate(engine);
                var eclMetadataField = ExtractEclMetadataField(ct, engine);
                if (eclMetadataField != null)
                {
                    var outputStr = package.GetByName("Output").GetAsString();
                    var entityModel = JsonConvert.DeserializeObject<EntityModelData>(outputStr, DataModelBinder.SerializerSettings);

                    ProcessMetadata(entityModel, eclMetadataField);

                    // Re-serialize the DXA JSON payload
                    //
                    package.PushItem(Package.OutputName,
                        package.CreateStringItem(ContentType.Text,
                            JsonSerialize(entityModel, IsPreview(engine), DataModelBinder.SerializerSettings)));
                }
            }
            
        }

        /// <summary>
        /// Extract metadata fields recursively from page regions. 
        /// </summary>
        /// <param name="regions"></param>
        /// <param name="eclMetadataFields"></param>
        /// <param name="engine"></param>
        private void ExtractMetadataFieldsFromRegions(IList<IRegion> regions, IList<EclMetadataField> eclMetadataFields, Engine engine)
        {
            foreach (var region in regions)
            {
                foreach (var cp in region.ComponentPresentations)
                {
                    ExtractMetadataFieldsFromComponentPresentation(cp, eclMetadataFields, engine);
                }

                ExtractMetadataFieldsFromRegions(region.Regions, eclMetadataFields, engine);
            }
            
        }

        /// <summary>
        /// Extract metadata field from component presentation.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="eclMetadataFields"></param>
        /// <param name="engine"></param>
        private void ExtractMetadataFieldsFromComponentPresentation(ComponentPresentation cp,
            IList<EclMetadataField> eclMetadataFields, Engine engine)
        {
            log.Debug("Checking CP: " + cp.Component.Id);
            EclMetadataField eclMetadataField = ExtractEclMetadataField(cp.ComponentTemplate, engine);
            if (eclMetadataField != null)
            {
                log.Debug("Found ECL metadata for field: " + eclMetadataField.FieldName);
                eclMetadataField.EntityId = cp.Component.Id.ItemId.ToString();
                eclMetadataFields.Add(eclMetadataField);
            }
        }
        
        /// <summary>
        /// Extract ECL Metadata field (if any) from component template.
        /// </summary>
        /// <param name="componentTemplate"></param>
        /// <param name="engine"></param>
        /// <returns></returns>
        private EclMetadataField ExtractEclMetadataField(ComponentTemplate componentTemplate, Engine engine)
        {
            var ctMetadata = new ItemFields(componentTemplate.Metadata, componentTemplate.MetadataSchema);

            // Loop through the metadata fields. For now just extract metadata from the first found ECL item
            //
            foreach (var metadataField in ctMetadata)
            {
                if (metadataField is MultimediaLinkField mmLinkField)
                {
                    if (mmLinkField.Values.Count > 0)
                    {
                        var eclComponent = mmLinkField.Values.First();
                        if (IsEclItem(eclComponent))
                        {
                            log.Debug("Found ECL link!");
                            IContentLibraryContext eclContext;
                            IContentLibraryMultimediaItem eclItem = GetEclItem(eclComponent.Id, engine.GetSession(),
                                out eclContext);
                            using (eclContext)
                            {
                                if (!string.IsNullOrEmpty(eclItem.MetadataXml))
                                {
                                    var externalMetadataDoc = new XmlDocument();
                                    externalMetadataDoc.LoadXml(eclItem.MetadataXml);
                                    return new EclMetadataField
                                    {
                                        FieldName = mmLinkField.Name,
                                        ExternalMetadata = externalMetadataDoc.DocumentElement
                                    };
                                    
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Process ECL metadata in entity model
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="eclMetadataField"></param>
        private void ProcessMetadata(EntityModelData entityModel, EclMetadataField eclMetadataField)
        {
            if (eclMetadataField.Processed)
            {
                return;
            }
            if (entityModel.MvcData.Parameters == null)
            {
                entityModel.MvcData.Parameters = new Dictionary<string, string>();
            }
                
            foreach (var metadataField in eclMetadataField.ExternalMetadata.ChildNodes)
            {
                if (metadataField is XmlElement metadataFieldElement)
                {
                    // TODO: Make sure there is no inner elements here
                    
                    if (!string.IsNullOrEmpty(metadataFieldElement.InnerText))
                    {
                        entityModel.MvcData.Parameters.Add(metadataFieldElement.Name, metadataFieldElement.InnerText);
                        eclMetadataField.Processed = true;
                    }
                }
            }

            // Remove the ECL link field from the CT metadata (as this can't be deserialized on the DXA side)
            //
            entityModel.ComponentTemplate.Metadata.Remove(eclMetadataField.FieldName);
        }

        /// <summary>
        /// Process entity MVC view metadata recursively in regions.
        /// </summary>
        /// <param name="regionModel"></param>
        /// <param name="eclMetadataFields"></param>
        private void ProcessRegionModel(RegionModelData regionModel, IList<EclMetadataField> eclMetadataFields)
        {
            if (regionModel.Entities != null)
            {
                foreach (var entityModel in regionModel.Entities)
                {
                    var eclMetadataField = eclMetadataFields.FirstOrDefault(f => f.EntityId.Equals(entityModel.Id) && f.Processed == false);
                    if (eclMetadataField != null)
                    {
                        log.Debug("Found matching ECL metadata field for ID: " + entityModel.Id);
                        ProcessMetadata(entityModel, eclMetadataField);
                    }
                }
            }

            if (regionModel.Regions != null)
            {
                foreach (var nestedRegionModel in regionModel.Regions)
                {
                    ProcessRegionModel(nestedRegionModel, eclMetadataFields);
                }
            }
        }

        /// <summary>
        /// Returns the Page object that is defined in the package for this template.
        /// </summary>
        /// <returns>the page object that is defined in the package for this template.</returns>
        private Page GetPage(Engine engine, Package package)
        {
            //first try to get from the render context
            RenderContext renderContext = engine.PublishingContext?.RenderContext;
            Page contextPage = renderContext?.ContextItem as Page;
            if (contextPage != null)
            {
                return contextPage;
            }

            Item pageItem = package.GetByType(ContentType.Page);
            if (pageItem == null)
            {
                return null;
            }

            return (Page) engine.GetObject(pageItem);
        }

        /// <summary>
        /// Returns the Template from the resolved item if it's a Component Template
        /// </summary>
        /// <returns>A Component Template or <c>null</c></returns>
        protected ComponentTemplate GetComponentTemplate(Engine engine) 
            => engine.PublishingContext.ResolvedItem.Template as ComponentTemplate;

        /// <summary>
        /// Gets whether the item is being rendered as part of CM Preview.
        /// </summary>
        protected bool IsPreview(Engine engine)
        {
            return (engine.RenderMode == RenderMode.PreviewDynamic) || (engine.RenderMode == RenderMode.PreviewStatic);
        }

        protected static bool IsEclItem(Component component) =>
            (component.BinaryContent != null) && (component.BinaryContent.MultimediaType.MimeType == EclMimeType);
        
        private IContentLibraryMultimediaItem GetEclItem(string eclStubComponentId, Session session, out IContentLibraryContext eclContext)
        {
            var eclSession =  SessionFactory.CreateEclSession(session);
            IEclUri eclUri = eclSession.TryGetEclUriFromTcmUri(eclStubComponentId);
            if (eclUri == null)
            {
                throw new ApplicationException("Unable to get ECL URI for ECL Stub Component: " + eclStubComponentId);
            }

            eclContext = eclSession.GetContentLibrary(eclUri);
            // This is done this way to not have an exception thrown through GetItem, as stated in the ECL API doc.
            // The reason to do this, is because if there is an exception, the ServiceChannel is going into the aborted state.
            // GetItems allows up to 20 (depending on config) connections. 
            IList<IContentLibraryItem> eclItems = eclContext.GetItems(new[] { eclUri });
            IContentLibraryMultimediaItem eclItem = (eclItems == null) ? null : eclItems.OfType<IContentLibraryMultimediaItem>().FirstOrDefault();
            if (eclItem == null)
            {
                eclContext.Dispose();
                throw new ApplicationException($"ECL item '{eclUri}' not found (TCM URI: '{eclStubComponentId}')");
            }

            return eclItem;
        }
        
        /// <summary>
        /// Serialize object to JSON.
        /// </summary>
        /// <param name="objectToSerialize"></param>
        /// <param name="prettyPrint"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected string JsonSerialize(object objectToSerialize, bool prettyPrint = false,
            JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
            }

            Newtonsoft.Json.Formatting jsonFormatting = prettyPrint
                ? Newtonsoft.Json.Formatting.Indented
                : Newtonsoft.Json.Formatting.None;

            return JsonConvert.SerializeObject(objectToSerialize, jsonFormatting, settings);         
        }
    }

    /// <summary>
    /// ECL Metadata Field
    /// </summary>
    class EclMetadataField
    {
        public string EntityId { get; set; }
        public string FieldName { get; set; }
        public XmlElement ExternalMetadata { get; set; }

        public bool Processed { get; set; } = false;
    }
}
