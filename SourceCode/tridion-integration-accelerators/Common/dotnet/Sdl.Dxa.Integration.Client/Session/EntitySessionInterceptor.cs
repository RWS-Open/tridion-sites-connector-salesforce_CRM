using System.Linq;
using Sdl.Dxa.Integration.Client.Processor;
using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Integration.Client
{
    public class EntitySessionInterceptor : IIntegrationInterceptor
    {
        private string[] _entityTypes;
        
        public EntitySessionInterceptor(string[] entityTypes)
        {
            _entityTypes = entityTypes;
        }
        
        public void PreProcess(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType)
        {
        }

        public void PostProcess(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType)
        {
            if (entityType != null && _entityTypes.Contains(entityType))
            {
                EntitySession.Instance.SaveEntity(identity.Type, entity);
            }
        }
    }
}