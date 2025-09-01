using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Modules.Crm.Models;
using Sdl.Web.Common.Logging;
using Sdl.Web.Common.Models;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    /// <summary>
    /// CRM Visitor Tracking Handler.
    /// </summary>
    public class CRMVisitorTrackingHandler : ITrackingHandler
    {
        private readonly string _trackingEntityName;
        private readonly IDictionary<string,string> _trackingFieldMappings = new Dictionary<string, string>();
        private readonly bool _trackAnonymousVisitors;

        private const string ANONYMOUS_VISITOR_TYPE = "AnonymousVisitor";
        
        public CRMVisitorTrackingHandler()
        {
         //   System.Diagnostics.Debugger.Launch();
         //   System.Diagnostics.Debugger.Break();

            // TODO: Support different tracking entities across namespaces?
            _trackingEntityName = WebConfigurationManager.AppSettings["crm-tracking-entity-name"];
            var fieldMappings = WebConfigurationManager.AppSettings["crm-tracking-field-mappings"];
            if (fieldMappings != null)
            {
                var tokens = fieldMappings.Split(new char[] {',', ' ', '='}, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < tokens.Length; i++)
                {
                    if (i + 1 < tokens.Length)
                    {
                        _trackingFieldMappings.Add(tokens[i], tokens[i + 1]);
                    }
                }
            }

            var trackAnonymousVisitorsStr = WebConfigurationManager.AppSettings["crm-track-anonymous-visitors"];
            if (trackAnonymousVisitorsStr != null)
            {
                _trackAnonymousVisitors = Boolean.Parse(trackAnonymousVisitorsStr);
            }
        }
        
        public string CreateTrackingKey(DynamicEntity visitor, string namespaceId, string type)
        {
            return null;
        }

        public string GetVisitorTrackingKey(string namespaceId, string type)
        {
            return null;
        }

        // TODO: Should we have a dedicated loader that can be called by authentication controllers etc???
        // It also makes for scenarios when you only want to load external data into the session using for example cookie values etc 
        // Or just have a dedicated Marketo module to start with? Just to make it simple...
        public DynamicEntity LoadVisitor(string trackingKey, string namespaceId, string type)
        {
            var visitor = EntitySession.Instance.GetEntity(type);
            
            if (visitor == EntitySessionConstants.NotAvailable)
            {
                if (_trackAnonymousVisitors)
                {
                    return EntitySession.Instance.GetEntity(ANONYMOUS_VISITOR_TYPE);
                }
                return null;
            }

            if (visitor == null)
            {
                Log.Info("Loading visitor from CRM...");
                
                // TODO: Store anonymous tracking records either in the session or as an anonymous visitor record in Salesforce connected to the tracking cookie
                // TODO: Should the CRM contact be merged if existing record is already there?
                
                // Load visitor from CRM
                //
                var result = IntegrationApiClientProvider.Instance.Client.QueryEntities(
                    new EntityFilter
                    {
                        SearchText = trackingKey,
                        EntityType = type,
                        Context = RootEntity.CreateRootEntityIdentity(namespaceId, null)
                    });

                if (result.DynamicEntities?.Count() > 0)
                {
                    // Pick the most recent item if there are several CRM entities associated with the same tracking key.
                    // Should normally only occur in demo environments
                    //
                    visitor = result.DynamicEntities.Last();
                    
                    // Save the loaded entity into the session
                    //
                    EntitySession.Instance.SaveEntity(type, visitor);
                }
                else
                {
                    if (_trackAnonymousVisitors)
                    {
                        Log.Info("Creating an anonymous visitor...");
                        EntitySession.Instance.SaveEntity(ANONYMOUS_VISITOR_TYPE, new AnonymousVisitor());
                    }
                    
                    // Set the entity to not available to avoid trying to load it again
                    // TODO: Check this once more if this is the right way to do this
                    EntitySession.Instance.SaveEntity(type, EntitySessionConstants.NotAvailable);
                }
                
            }

            return visitor;
        }

        public void TrackVisitor(DynamicEntity visitor, PageModel pageModel, IList<string> trackedCategories)
        {
           
            Log.Debug("Tracking page: " + pageModel.Url);
            
            // Create a new tracking record in CRM
            //
            DynamicEntity trackingEntity = new DynamicEntity();
            trackingEntity.SetField(GetTrackingEntityFieldName("sessionId"), EntitySession.Instance.SessionId);
            trackingEntity.SetField(GetTrackingEntityFieldName("siteUrl"), pageModel.Url);
            trackingEntity.SetField(GetTrackingEntityFieldName("categories"), trackedCategories);

            if (visitor is AnonymousVisitor anonymousVisitor)
            {
                Log.Info("Tracking anonymous visits on URL: " + pageModel.Url);
                
                // For now store the tracking records in the session. 
                // Should be stored in CRM as a record and merged when user become known (registration, login etc)
                //
                anonymousVisitor.TrackingRecords.Add(trackingEntity);
            }
            else
            {
                // TODO: Do this as as an sync task

                if (_trackAnonymousVisitors)
                {
                    CheckAndHandleAnonymousTrackingRecords(visitor);
                }

                var createdId = IntegrationApiClientProvider.Instance.Client.CreateEntity(visitor.Identity,
                    _trackingEntityName,
                    trackingEntity);
                Log.Debug($"Created a new tracking record with ID: {createdId?.Id}");
                
            }

        }

        private string GetTrackingEntityFieldName(string name)
        {
            if (_trackingFieldMappings.TryGetValue(name, out string entityFieldName))
            {
                return entityFieldName;
            }

            // No mapping available. Assume the 1:1 mapping of the names
            //
            return name;
        }

        private void CheckAndHandleAnonymousTrackingRecords(DynamicEntity visitor)
        {
            if (EntitySession.Instance.GetEntity(ANONYMOUS_VISITOR_TYPE) is AnonymousVisitor anonymousVisitor)
            {
                lock (anonymousVisitor)
                {
                    Log.Info($"Storing pending tracking records ({anonymousVisitor.TrackingRecords.Count}) from anonymous visitor...");
                    foreach (var trackingRecord in anonymousVisitor.TrackingRecords)
                    {
                        var createdId = IntegrationApiClientProvider.Instance.Client.CreateEntity(visitor.Identity,
                            _trackingEntityName,
                            trackingRecord);
                        Log.Info($"Created a new tracking record with ID: {createdId?.Id}");
                    }
                    anonymousVisitor.TrackingRecords.Clear();
                }

                EntitySession.Instance.SaveEntity(ANONYMOUS_VISITOR_TYPE, null);
            }
        }
    }
}