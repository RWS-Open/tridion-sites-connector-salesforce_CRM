using Tridion.ConnectorFramework.Connector.SDK;
using Tridion.ConnectorFramework.Contracts;
using Tridion.ConnectorFramework.Contracts.Schema;

namespace Sdl.Dxa.Integration.Client
{
    // TODO: Define Async methods here as well!!
    
    public interface IIntegrationApi
    {
        DynamicEntity GetEntity(IEntityIdentity identity);
        DynamicEntity GetEntity(string eclUri);
        
        DynamicEntityPaginatedList ListEntities(IEntityIdentity parentIdentity, IPaginationData paginationData = null);
        DynamicEntityPaginatedList QueryEntities(IEntityFilter filter, IPaginationData paginationData = null);
        IEntityIdentity CreateEntity(IEntityIdentity parentIdentity, string type, DynamicEntity entity);
        DynamicEntity CreateEntityWithResponse(IEntityIdentity parentIdentity, string type, DynamicEntity entity);
        void UpdateEntity(IEntityIdentity identity, DynamicEntity entity);
        DynamicEntity UpdateEntityWithResponse(IEntityIdentity identity, DynamicEntity entity);
        void DeleteEntity(IEntityIdentity identity);

        IEntitySchema GetEntitySchema(string typeName);

        // Have a method to DeSerializeEntity<Entity Class>(byte[] bytes) ???
        //

        // Get namespaces...
    }
}