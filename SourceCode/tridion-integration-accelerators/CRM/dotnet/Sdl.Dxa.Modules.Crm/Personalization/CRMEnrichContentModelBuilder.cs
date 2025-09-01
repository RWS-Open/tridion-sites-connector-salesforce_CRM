using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Sdl.Dxa.Integration.Client;
using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Logging;
using Sdl.Web.Common.Models;
using Sdl.Web.DataModel;
using Sdl.Web.Tridion.Mapping;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ContentDelivery.AmbientData;

namespace Sdl.Dxa.Modules.Crm.Personalization
{
    // TODO: Is this really CRM specific or not???
    // TODO: Make it possible to configure to read ADF claims as well
    
    // On which articles should it trigger on? Should that be configurable???
    public class CRMEnrichContentModelBuilder : IEntityModelBuilder
    {
        private string _entityName;
        
        // TODO: Refactor this into different handler, like for ADF and for CRM
        // Generalize this into a map?
        // For now just patch in support for ADF claims
        //
        private string _adfClaimPrefix;

        public CRMEnrichContentModelBuilder()
        {
            _entityName = WebConfigurationManager.AppSettings["crm-personalization-entity-name"];
            _adfClaimPrefix = WebConfigurationManager.AppSettings["crm-personalization-adf-prefix"];
        }
        
        public void BuildEntityModel(ref EntityModel entityModel, EntityModelData entityModelData, Type baseModelType,
            Localization localization)
        {
            ProcessEntity(entityModel);
        }

        private void ProcessEntity(EntityModel entityModel)
        {

            // TODO: Is there several sessions ??
            if (_entityName == null && _adfClaimPrefix == null)
            {
                return;
            }
            
            var entity = EntitySession.Instance.GetEntity(_entityName); // TODO: Have this configurable....
            if (entity == null)
            {
                // Try to load entity via ADF (temp fix)
                //
                entity = LoadEntityFromADF();
                
                if (entity == null) {
                    // No entity available -> no action TODO: Should we try to load it here??
                    return;
                }
            }
            
            Log.Debug("Processing entity: " + entityModel.GetType().Name);
            
            var type = entityModel.GetType();
            foreach (var property in type.GetProperties())
            {
                var declaredInType = property.DeclaringType;

                // Skip standard entity/media fields (e.g. ID, filename etc)
                //
                if (declaredInType == typeof(ViewModel) || declaredInType == typeof(EntityModel) || declaredInType == typeof(MediaItem))
                {
                    continue;
                }
                if (property.PropertyType == typeof(string))
                {
                    string content = (string)property.GetValue(entityModel);
                    if (content != null)
                    {
                        var processedContent = VariableSubstituter.ProcessContent(content, c => entity.GetField(c)?.ToString());
                        // Problem with header caching???
                        if (!processedContent.Equals(content))
                        {
                            property.SetValue(entityModel, processedContent);
                        }
                    }
                }
                else if (property.PropertyType == typeof(RichText))
                {
                    RichText richText = (RichText)property.GetValue(entityModel);
                    if (richText != null)
                    {
                        var content = richText.ToString();
                        Log.Debug("Processing rich text: " + content);
                        var processedContent = VariableSubstituter.ProcessContent(content, c => entity.GetField(c)?.ToString());
                        if (!processedContent.Equals(content))
                        {
                            property.SetValue(entityModel, new RichText(processedContent));
                        }
                    }
                }

                // TODO: Add support single nested items!!
                else if (IsList(property.PropertyType))
                {
                    if (property.GetValue(entityModel) is IList list && list.Count > 0)
                    {
                        if (list[0] != null && list[0].GetType().IsSubclassOf(typeof(EntityModel)) )
                        {
                            foreach (var listEntry in list)
                            {
                                ProcessEntity((EntityModel)listEntry);
                            }

                        }
                    }
                }
            }
            
        }
        
        private static bool IsList(Type type)
        {
            var targetType = typeof(IList<>);
            return type.GetInterfaces().Any(i => i.IsGenericType
                                                 && i.GetGenericTypeDefinition() == targetType);
        }

        private DynamicEntity LoadEntityFromADF()
        {
            if (_adfClaimPrefix != null)
            {
                DynamicEntity adfEntity = new DynamicEntity();
                var allClaims = AmbientDataContext.CurrentClaimStore.GetAll();
                foreach (var claim in allClaims)
                {
                    var claimUri = claim.Key.ToString();
                    if (claimUri.StartsWith(_adfClaimPrefix))
                    {
                        if (!IsList(claim.Value.GetType()))
                        {
                            adfEntity.SetField(claimUri, claim.Value);
                        }
                    }
                }

                return adfEntity;
            }

            return null;
        }
    }
}