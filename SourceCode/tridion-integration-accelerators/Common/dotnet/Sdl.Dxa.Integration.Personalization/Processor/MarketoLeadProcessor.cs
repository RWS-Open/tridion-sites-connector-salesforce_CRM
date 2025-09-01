using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Client.Processor;
using Sdl.Web.Common.Logging;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ContentDelivery.AmbientData;

namespace Sdl.Dxa.Integration.Personalization.Processor
{
    /// <summary>
    /// Marketo Lead Processor
    /// </summary>
    public class MarketoLeadProcessor : IRequestProcessor
    {
        private const string CookiesUri = "taf:request:cookies";
        private const string MarketoCookieName = "_mkto_trk"; // TODO: Have this configurable
        private const string EntityType = "MarketoLead";
        
        private string _namespaceId;
        private bool _enabled;

        public MarketoLeadProcessor()
        {
            _namespaceId = WebConfigurationManager.AppSettings["request-processor-marketo-namespace"];
            var enableStr = WebConfigurationManager.AppSettings["request-processor-marketo-enabled"];
            if (!Boolean.TryParse(enableStr, out _enabled))
            {
                _enabled = false;
            }

            if (_enabled && _namespaceId == null)
            {
                Log.Warn("No namespace ID is configured for the Marketo Request Processor. The request processor will not be enabled.");
                _enabled = false;
            }
            
            if (_enabled)
            {
                Log.Info("Enabling Marketo Lead Processor...");
            }
        }
        
        public void PreProcess(HttpRequest request)
        {
            if (!_enabled)
            {
                return;
            }
            var cookieValue = GetCookieValue(MarketoCookieName);

            if (!string.IsNullOrEmpty(cookieValue))
            {
                if (EntitySession.Instance.GetEntity(EntityType) == null)
                {
                    var leadEntity = LoadLead(cookieValue);
                    SaveLead(leadEntity);
                }
            }
        }

        public void PostProcess(HttpRequest request, HttpResponse response)
        {
            //
        }

        public string GetCookieValue(string cookieName)
        {
            var cookieClaims = AmbientDataContext.CurrentClaimStore.Get<Dictionary<string, string>>(CookiesUri);

            if (cookieClaims != null && cookieClaims.TryGetValue(cookieName, out string cookieValue))
            {
                return cookieValue;
            }

            return string.Empty;
        }

        public DynamicEntity LoadLead(string cookieValue)
        {
            Log.Info("Loading Marketo Lead using cookie: " + cookieValue);
            var leadFilter = new EntityFilter
            {
                SearchText = cookieValue,
                EntityType = EntityType,
                Context = RootEntity.CreateRootEntityIdentity(_namespaceId, null),
            };

            try
            {
                var result = IntegrationApiClientProvider.Instance.Client.QueryEntities(leadFilter);

                if (result.DynamicEntities?.Count() > 0)
                {
                    return result.DynamicEntities.LastOrDefault();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "No Marketo lead could be loaded from given cookie.");
                
                // Mark it as not available to avoid to try loading the entity for next request in the current session
                //
                EntitySession.Instance.SaveEntity(EntityType, EntitySessionConstants.NotAvailable);
            } 

            return null;
        }

        public void SaveLead(DynamicEntity leadEntity)
        {
            EntitySession.Instance.SaveEntity(EntityType, leadEntity);
        }
    }
}