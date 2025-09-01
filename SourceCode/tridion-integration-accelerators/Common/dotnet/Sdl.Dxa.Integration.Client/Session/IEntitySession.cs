using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Client
{
    // TODO: Triggered by event triggers
    // TODO: Be able to keep state stored in cookies as well (serialized in JSON)
    
    public interface IEntitySession
    {
        // TODO: Have some kind of scope of stuff we add here?? Named, Singleton etc

        // TODO: Add namespace here as well!!
        
        string SessionId { get; } // TODO: Do we need session ID to more than just tracking??
        
        DynamicEntity GetEntity(string type, string identity = null);
        void SaveEntity(string type, DynamicEntity entity, string identity = null);
    }
}