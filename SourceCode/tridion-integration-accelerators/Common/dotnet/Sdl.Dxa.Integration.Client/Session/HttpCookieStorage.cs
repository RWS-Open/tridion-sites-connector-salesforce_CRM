using Sdl.Web.Mvc.Configuration;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Client
{
    public class HttpCookieStorage : IEntitySessionStorage
    {
        public string SessionId => null; // TODO: TO BE IMPLEMENTED

        public void SaveEntity(string type, DynamicEntity entity, string id = null)
        {
            throw new System.NotImplementedException();
        }

        public DynamicEntity LoadEntity(string type, string id = null)
        {
            
            
            throw new System.NotImplementedException();
        }
    }
}