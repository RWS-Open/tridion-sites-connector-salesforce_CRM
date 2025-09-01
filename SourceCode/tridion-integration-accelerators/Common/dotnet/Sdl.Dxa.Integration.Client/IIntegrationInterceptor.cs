using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;

namespace Sdl.Dxa.Integration.Client.Processor
{
    /// <summary>
    /// Integration Interceptor interface.
    /// </summary>
    public interface IIntegrationInterceptor
    {
        void PreProcess(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType); 
        void PostProcess(IEntityIdentity identity, DynamicEntity entity, string namespaceId, string entityType, EntityOperationType operationType);
    }
}