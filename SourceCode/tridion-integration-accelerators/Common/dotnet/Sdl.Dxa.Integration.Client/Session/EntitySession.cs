using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Client
{
    
    public class EntitySession : IEntitySession
    {
        private static EntitySession _instance;
        
        // TODO: Have listeners when entity gets populated? So we can re-set ADF claims etc??? 
        
        private static IEntitySessionStorage[] _sessionStorages =
        {
            new HttpSessionStorage(), // TODO: Make this configurable
            new ADFStorage()
        };

        public static EntitySession Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EntitySession();
                }

                return _instance;
            }
        }
        private EntitySession()
        {
        }

        public string SessionId
        {
            get
            {
                foreach (var storage in _sessionStorages)
                {
                    var sessionId = storage.SessionId;
                    if (sessionId != null)
                    {
                        return sessionId;
                    }
                }

                return null;
            }
        }

        public DynamicEntity GetEntity(string type, string identity = null)
        {
            foreach (var storage in _sessionStorages)
            {
                var entity = storage.LoadEntity(type, identity);
                if (entity != null)
                {
                    return entity;
                }
            }

            return null;
        }

        public void SaveEntity(string type, DynamicEntity entity, string identity = null)
        {
            foreach (var storage in _sessionStorages)
            {
                storage.SaveEntity(type, entity, identity);
            }
        }
        
    }
}