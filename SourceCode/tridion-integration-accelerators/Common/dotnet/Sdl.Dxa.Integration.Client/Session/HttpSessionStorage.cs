using System;
using System.Web;
using System.Web.Configuration;
using System.Web.WebPages;
using Tridion.ConnectorFramework.Connector.SDK;

namespace Sdl.Dxa.Integration.Client
{
    public class HttpSessionStorage : IEntitySessionStorage
    {
        private long _storageExpiryTime = -1; // Expiry timeout in seconds
        
        public HttpSessionStorage()
        {
            var expiryTimeStr = WebConfigurationManager.AppSettings["session-storage-expiry-time"];
            if (expiryTimeStr != null && expiryTimeStr.IsInt())
            {
                _storageExpiryTime = Int64.Parse(expiryTimeStr);
            }
        }
        
        public string SessionId => HttpContext.Current.Session.SessionID;

        public void SaveEntity(string type, DynamicEntity entity, string id = null)
        {
            var storageKey = GetStorageKey(type, id);
            HttpContext.Current.Session[storageKey] = entity;
            if (_storageExpiryTime != -1)
            {
                HttpContext.Current.Session[storageKey + ":expiry"] = DateTime.Now.AddSeconds(_storageExpiryTime);
            }
        }

        public DynamicEntity LoadEntity(string type, string id = null)
        {
            var storageKey = GetStorageKey(type, id);
            var expiryTime = (DateTime?) HttpContext.Current.Session[storageKey + ":expiry"];
            if (expiryTime != null && expiryTime < DateTime.Now)
            {
                HttpContext.Current.Session[storageKey] = null;
                HttpContext.Current.Session[storageKey + ":expiry"] = null;
                return null;
            }
            return (DynamicEntity) HttpContext.Current.Session[storageKey];     
        }

        private string GetStorageKey(string type, string id)
        {
            return type + ":" + id;
        }
    }
}