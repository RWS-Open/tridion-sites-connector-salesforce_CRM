using System;
using System.Web;
using System.Web.Configuration;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Client.Processor;
using Sdl.Web.Common.Logging;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Personalization.Processor
{
    /// <summary>
    /// SFMC Contact Processor
    /// </summary>
    public class SFMCContactProcessor : IRequestProcessor
    {
        private const string ContactKeyUrlName = "ContactKey";
        private const string EntityType = "Contact";
        
        private string _namespaceId;
        private bool _enabled;
        
        public SFMCContactProcessor()
        {
            _namespaceId = WebConfigurationManager.AppSettings["request-processor-sfmc-namespace"];
            var enableStr = WebConfigurationManager.AppSettings["request-processor-sfmc-enabled"];
            if (!Boolean.TryParse(enableStr, out _enabled))
            {
                _enabled = false;
            }

            if (_enabled && _namespaceId == null)
            {
                Log.Warn("No namespace ID is configured for the SFMC Request Processor. The request processor will not be enabled.");
                _enabled = false;
            }
            
            if (_enabled)
            {
                Log.Info("Enabling SFMC Contact Processor...");
            }
        }
        
        public void PreProcess(HttpRequest request)
        {
            if (!_enabled)
            {
                return;
            }

            var contactKey = request.Params.Get(ContactKeyUrlName);
            if (contactKey != null)
            {
                // Load contact into session
                //
                var contact = LoadContact(contactKey);
                if (contact is null)
                {
                    // Mark it as not available to avoid to try loading the entity for next request in the current session
                    //
                    EntitySession.Instance.SaveEntity(EntityType, EntitySessionConstants.NotAvailable);
                    return;
                }
                EntitySession.Instance.SaveEntity(EntityType, contact);
                
                HttpContext.Current.Items["RedirectUrlAfterProcessing"] = request.Url.GetLeftPart(UriPartial.Path);
            }
        }

        public void PostProcess(HttpRequest request, HttpResponse response)
        {
            // TODO: Should this a generic functionality in the request processor?
            if (HttpContext.Current.Items["RedirectUrlAfterProcessing"] is string redirectUrl)
            {
                response.Redirect(redirectUrl);
            }
        }
        
        public DynamicEntity LoadContact(string contactKey)
        {
            Log.Info("Loading SFMC contact using contact key URL param: " + contactKey);
            try
            {
                var result = IntegrationApiClientProvider.Instance.Client.GetEntity(
                    new EntityIdentity(contactKey)
                    {
                        NamespaceId = _namespaceId,
                        Type = EntityType
                    });

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not load SFMC contact");
                return null;
            }
        }

    }
}