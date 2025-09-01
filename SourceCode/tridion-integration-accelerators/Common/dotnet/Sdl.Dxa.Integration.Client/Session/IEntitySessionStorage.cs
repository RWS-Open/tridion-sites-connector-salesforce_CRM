using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Client
{
    public interface IEntitySessionStorage
    {
        string SessionId { get; }
        void SaveEntity(string type, DynamicEntity entity, string id = null);
        DynamicEntity LoadEntity(string type, string id = null);
    }
}