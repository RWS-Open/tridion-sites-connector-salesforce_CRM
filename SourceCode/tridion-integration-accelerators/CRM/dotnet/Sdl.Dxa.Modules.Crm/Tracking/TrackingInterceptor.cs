using System.Collections.Generic;
using System.Linq;
using Sdl.Dxa.Integration.Client;
using Sdl.Dxa.Integration.Client.Processor;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Modules.Crm.Tracking
{
    public class TrackingInterceptor : IIntegrationInterceptor
    {
        private IList<TrackingNamespaceEntityType> _entityTypes = TrackingNamespaceEntityType.ReadConfiguredTypes();
        
        
        public void PreProcess(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType)
        {
            if (_entityTypes.Any(t => t.NamespaceId.Equals(namespaceId) && t.TypeName.Equals(entityType))) 
            {
                foreach (var trackingHandler in DIRegisty.GetList<ITrackingHandler>())
                {
                    var trackingKey = trackingHandler.CreateTrackingKey(entity, namespaceId, entityType);
                    if (trackingKey != null)
                    {
                        // replace with alina_saleforce__WebTrackingId   trackingKey
                        // TODO: Have this configurable per entity type
                        entity.SetField("trackingKey", trackingKey);
                    }
                }
            }
        }

        public void PostProcess(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType)
        {
        }
    }
}